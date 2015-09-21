using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics.Contracts;

namespace TMResult
{
    public class Infomation
    {
        public async Task<IEnumerable<int>> GetLatestGameIDAsync()
        {
            Contract.Ensures(Contract.Result<Task<IEnumerable<int>>>() != null);
            
            HttpClient client = new HttpClient();
            string latest;
            try {
                latest = await client.GetStringAsync("http://tankmatch.net/result/last_10.txt");
            } catch (HttpRequestException) {
                return null;
            } catch (TaskCanceledException) {
                return null;
            }
            return latest.Split('\n').Where(s => s.Length != 0).Select(int.Parse);
        }
    }
}
