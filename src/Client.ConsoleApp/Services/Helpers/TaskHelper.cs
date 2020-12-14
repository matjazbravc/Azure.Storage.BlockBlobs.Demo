namespace Client.ConsoleApp.Services.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TaskHelper : ITaskHelper
    {
        public Task ForEachAsync<T>(IEnumerable<T> source, int partitionCount, Func<T, Task> body)
        {
            return Task.WhenAll(
                Partitioner
                .Create(source)
                .GetPartitions(partitionCount)
                .Select(partition => Task.Run(async () =>
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current).ConfigureAwait(false);
                        }
                    }
                })));
        }
    }
}
