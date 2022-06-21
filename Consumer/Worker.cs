using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Services;

namespace Consumer
{
    public class Worker
    {
        private readonly IConfiguration configuration;
        private readonly IMQService _mQService;

        public Worker(IConfiguration configuration, IMQService mQService)
        {
            this.configuration = configuration;
            _mQService = mQService;
        }

        public void DoWork()
        {
            var message = _mQService.ConsumeMQ();
            Console.WriteLine(message);
        }
    }
}
