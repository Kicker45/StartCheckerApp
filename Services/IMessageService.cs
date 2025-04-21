using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartCheckerApp.Services;
public interface IMessageService
{
    Task ShowMessageAsync(string message);
}
