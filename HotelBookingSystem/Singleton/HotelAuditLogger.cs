using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBookingSystem.Singleton
{
     public partial class HotelAuditLogger : Component
     {
          public HotelAuditLogger()
          {
               InitializeComponent();
          }

          public HotelAuditLogger(IContainer container)
          {
               container.Add(this);

               InitializeComponent();
          }
     }
}
