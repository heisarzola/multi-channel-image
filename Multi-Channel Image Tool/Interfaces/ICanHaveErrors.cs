using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Channel_Image_Tool.Interfaces
{
    public interface ICanHaveErrors
    {
        List<string> Errors { get; }
    }
}
