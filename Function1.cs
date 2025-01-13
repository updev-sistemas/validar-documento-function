using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp
{
    public static class ValidarDocumentoFunction
    {
        [FunctionName("ValidateCpfCnpj")]
        public static async Task<IActionResult> Run(
               [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
               ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string input = data?.document;

            if (string.IsNullOrWhiteSpace(input))
            {
                return new BadRequestObjectResult("Invalid input. Please provide a CPF or CNPJ.");
            }

            input = Regex.Replace(input, @"\D", "");

            if (IsValidCpf(input))
            {
                log.LogInformation($"Documento {input} do tipo CPF Válido.");
                return new OkObjectResult("Documento CPF Válido.");
            }
            else if (IsValidCnpj(input))
            {
                log.LogInformation($"Documento {input} do tipo CNPJ Válido.");
                return new OkObjectResult("Documento CNPJ Válido.");
            }
            else
            {
                log.LogInformation($"Documento {input} inválido.");
                return new BadRequestObjectResult("Documento inválido.");
            }
        }

        private static bool IsValidCpf(string cpf)
        {
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0])) return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = tempCpf.Select((t, i) => (t - '0') * multiplicador1[i]).Sum();
            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCpf += digito1;
            soma = tempCpf.Select((t, i) => (t - '0') * multiplicador2[i]).Sum();
            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return cpf.EndsWith($"{digito1}{digito2}");
        }

        private static bool IsValidCnpj(string cnpj)
        {
            if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0])) return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = tempCnpj.Select((t, i) => (t - '0') * multiplicador1[i]).Sum();
            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCnpj += digito1;
            soma = tempCnpj.Select((t, i) => (t - '0') * multiplicador2[i]).Sum();
            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return cnpj.EndsWith($"{digito1}{digito2}");
        }
    }
}
