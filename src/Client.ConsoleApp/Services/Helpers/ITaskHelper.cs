namespace Client.ConsoleApp.Services.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITaskHelper
    {
        Task ForEachAsync<T>(IEnumerable<T> source, int partitionCount, Func<T, Task> body);
    }
}