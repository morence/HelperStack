using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EBayAPI.Infrastructure.Extensions
{
    public static class DelegateExtensions
    {
        #region 开启多线程执行 + void ExcuteSegmentsByMultiTasks(this Action<int> action, int taskSize, int pageSize, int pageCount)
        /// <summary>
        /// 开启多线程执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="taskSize">线程数量</param>
        /// <param name="pageSize">页码</param>
        /// <param name="pageCount">总数</param>
        public static void ExcuteSegmentsByMultiTasks(this Action<int> action, int taskSize, int pageSize, int pageCount)
        {
            if (action == null || pageCount <= 0 || pageSize <= 0)
                return;
            //总页数
            int totalPage = pageCount / pageSize;
            if (pageCount % pageSize > 0)
            {
                totalPage += 1;
            }
            Action<object> act = t => { action((int)t); };
            List<Task> taskList = new List<Task>();
            if (totalPage <= 0)
            {
                taskList.Add(new Task(act, 0, TaskCreationOptions.LongRunning));
            }
            else
            {
                for (int i = 0; i < totalPage; i++)
                {
                    taskList.Add(new Task(act, i, TaskCreationOptions.LongRunning));
                }
            }

            #region 并发执行线程

            if (taskSize <= 0)
            {
                taskList.ForEach(t => t.Start());
                Task.WaitAll(taskList.ToArray());
            }
            else
            {
                var taskPage = taskList.Count / taskSize + (taskList.Count % taskSize > 0 ? 1 : 0);

                for (int i = 0; i < taskPage; i++)
                {
                    var pageTasks = taskList.Skip(taskSize * i).Take(taskSize).ToList();

                    pageTasks.ForEach(t => t.Start());

                    Task.WaitAll(pageTasks.ToArray());
                }
            }

            #endregion
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalData">源数据</param>
        /// <param name="segmentSize">分割大小</param>
        /// <param name="taskSize">线程数</param>
        public static void ExcuteSegmentsByMultiTask<T>(this Action<List<T>> action, List<T> originalData, int segmentSize, int taskSize)
        {
            if (action == null || originalData == null || originalData.Count() == 0)
                return;

            List<Task> tasks = new List<Task>();

            Action<object> act = t => { action(t as List<T>); };

            if (segmentSize <= 0)
            {
                tasks.Add(new Task(act, originalData, TaskCreationOptions.LongRunning));
            }
            else
            {
                var page = originalData.Count() / segmentSize + (originalData.Count() % segmentSize > 0 ? 1 : 0);

                for (int i = 0; i < page; i++)
                {
                    var tmpDatas = originalData.Skip(i * segmentSize).Take(segmentSize).ToList();

                    tasks.Add(new Task(act, tmpDatas, TaskCreationOptions.LongRunning));
                }
            }

            #region 并发执行线程

            if (taskSize <= 0)
            {
                tasks.ForEach(t => t.Start());
                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                var taskPage = tasks.Count / taskSize + (tasks.Count % taskSize > 0 ? 1 : 0);

                for (int i = 0; i < taskPage; i++)
                {
                    var pageTasks = tasks.Skip(taskSize * i).Take(taskSize).ToList();

                    pageTasks.ForEach(t => t.Start());

                    Task.WaitAll(pageTasks.ToArray());
                }
            }

            #endregion
        }

        /// <summary>
        /// 已知总数并行分页获取数据
        /// </summary>
        /// <param name="action">执行取数据逻辑</param>
        /// <param name="taskSize">并发数量</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="totalCount">总数</param>
        public static ConcurrentQueue<T> ExcuteSegmentsByMultiTasks<T>(this Action<ConcurrentQueue<T>, int, int> action, int taskSize, int pageSize, int totalCount, Action<int> callBack = null)
        {
            var outputList = new ConcurrentQueue<T>();
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            //总页数
            int totalPage = totalCount / pageSize;
            if (totalCount % pageSize > 0)
            {
                totalPage += 1;
            }
            Enumerable.Range(1, totalPage).Select(i =>
            {
                queue.Enqueue(i);
                return i;
            }).ToArray();
            int completeCount = 0;
            object lockObj = new object();
            var tasks = Enumerable.Range(0, taskSize).Select(i =>
            {
                var task = Task.Factory.StartNew(() =>
                {
                    while (queue.TryDequeue(out int pageIndex))
                    {
                        action(outputList, pageIndex, pageSize);
                        if (callBack != null)
                        {
                            lock (lockObj)
                            {
                                completeCount++;
                                callBack(completeCount);
                            }
                        }
                    };
                });
                return task;
            }).ToArray();
            Task.WaitAll(tasks);
            return outputList;
        }

        /// <summary>
        /// 已知总数并行分页获取数据(内存中只保留60000条数据)
        /// </summary>
        /// <param name="func">执行取数据逻辑</param>
        /// <param name="taskSize">并发数量</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="totalCount">总数</param>
        /// <param name="singleCallback">回调更新获取数据进度</param>
        /// <param name="segCallback">批量回调写入Excel</param>
        public static void ExcuteSegmentsByMultiTasks<T>(this Func<int, int, List<T>> func, int taskSize, int pageSize, int totalCount, Action<double> singleCallback, Action<List<T>, List<string>, int> segCallback, int maxMemoryRowCount = 60000)
        {
            int segments = totalCount / maxMemoryRowCount;
            segments = (totalCount % maxMemoryRowCount > 0) ? segments + 1 : segments;
            int startIndex = 1;
            var files = new List<string>();
            int completeCount = 0;
            object lockObj = new object();
            for (int a = 0; a < segments; a++)
            {
                var outputList = new ConcurrentQueue<T>();
                ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
                //总页数
                int segCount = (totalCount - a * maxMemoryRowCount) < maxMemoryRowCount ? (totalCount - a * maxMemoryRowCount) : maxMemoryRowCount;
                int totalPage = segCount / pageSize;
                totalPage = (segCount % pageSize > 0) ? totalPage + 1 : totalPage;
                Enumerable.Range(startIndex, totalPage).Select(i =>
                {
                    queue.Enqueue(i);
                    return i;
                }).ToArray();
                startIndex += totalPage;
                var tasks = Enumerable.Range(0, taskSize).Select(i =>
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        while (queue.TryDequeue(out int pageIndex))
                        {
                            var pagingList = func(pageIndex, pageSize);
                            foreach (var paging in pagingList)
                            {
                                outputList.Enqueue(paging);
                            }

                            lock (lockObj)
                            {
                                completeCount++;

                                double percentage = Math.Round((double)(completeCount * pageSize) / totalCount, 2);
                                double remainder = (percentage * 100) % 2;
                                if (remainder == 0)
                                {
                                    singleCallback(percentage);
                                }
                            }
                        };
                    });
                    return task;
                }).ToArray();
                Task.WaitAll(tasks);
                segCallback(outputList.ToList(), files, segments);
            }
        }
    }
}
