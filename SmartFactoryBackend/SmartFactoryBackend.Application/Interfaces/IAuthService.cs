using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(string name, string lastname, string username, string email, string password);
        Task<string?> LoginAsync(string username, string password);
    }
}
