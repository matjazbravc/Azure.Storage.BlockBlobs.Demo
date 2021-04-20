using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.ConsoleApp.Services.Helpers
{
    public interface ITaskHelper
    {
        Task ForEachAsync<T>(IEnumerable<T> source, int partitionCount, Func<T, Task> body);
    }
}