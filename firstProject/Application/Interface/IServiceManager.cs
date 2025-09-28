namespace Application.Interface
{
    public interface IServiceManager
    {
        public IEmailService EmailService { get; }
        public ITokenService TokenService { get; }
        public IFunctionService FunctionService { get; }
    }
}
