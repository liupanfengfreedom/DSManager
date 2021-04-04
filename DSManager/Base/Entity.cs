using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    interface Entity
    {
          void Begin();
          void Update(float delta);
          void End();
    }
}
