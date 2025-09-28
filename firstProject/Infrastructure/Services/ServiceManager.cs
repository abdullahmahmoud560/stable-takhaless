using Application.Interface;
using AutoMapper;
using firstProject.ApplicationDbContext;
using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IEmailService> _emailService;
        private readonly Lazy<ITokenService> _tokenService;
        private readonly Lazy<IFunctionService> _functionService;
        public ServiceManager(UserManager<User> userManager,IMapper mapper,DB db,IHttpContextAccessor httpContextAccessor,HttpClient httpClient) 
        {
            _emailService = new Lazy<IEmailService>(() => new EmailService());
            _tokenService = new Lazy<ITokenService>(()=> new TokenService(userManager));
            _functionService = new Lazy<IFunctionService>(()=>new FunctionService(db,httpContextAccessor,httpClient));
        }
        public IEmailService EmailService => _emailService.Value;
        public ITokenService TokenService => _tokenService.Value;
        public IFunctionService FunctionService => _functionService.Value;
    }
}
