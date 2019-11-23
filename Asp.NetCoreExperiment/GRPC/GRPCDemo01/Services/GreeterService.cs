using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using GRPCDemo01Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace GRPCDemo01
{
    [Authorize("Permission")]
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        readonly PermissionRequirement _requirement;
        public GreeterService(ILogger<GreeterService> logger, PermissionRequirement requirement)
        {
            _requirement = requirement;
            _logger = logger;
        }  
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
        [AllowAnonymous]
        public override Task<UserTokenResponse> Login(UserRequest user, ServerCallContext context)
        {

            var isValidated = user.Username == "gsw" && user.Password == "111111";
            if (!isValidated)
            {
                return Task.FromResult(new UserTokenResponse()
                {
                    Message = "��֤ʧ��"
                });
            }
            else
            {
                //����ǻ����û�����Ȩ���ԣ�����Ҫ����û�;����ǻ��ڽ�ɫ����Ȩ���ԣ�����Ҫ��ӽ�ɫ
                var claims = new Claim[] {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString())
                };

                var token = JwtToken.BuildJwtToken(claims, _requirement);
                return Task.FromResult(new UserTokenResponse()
                {
                    Result = true,
                    Token = token.access_token
                }) ;

            }
        }
    }
}
