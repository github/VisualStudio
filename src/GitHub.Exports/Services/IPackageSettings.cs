using GitHub.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IPackageSettings
    {
        bool CollectMetrics { get; set; }
        void Save();
    }
}
