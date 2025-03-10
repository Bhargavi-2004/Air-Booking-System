using DTO;
using Models;

namespace Repository.Interfaces
{
    public interface IAccountRepository
    {
        Task<bool> Register(User user);
        Task<string> Login(LoginDTO loginDto);

    }
}
