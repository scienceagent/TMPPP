using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBookingSystem.Prototype
{
     public partial class RoomPrototypeRegistry : Component
     {
          public RoomPrototypeRegistry()
          {
               InitializeComponent();
          }

          public RoomPrototypeRegistry(IContainer container)
          {
               container.Add(this);

               InitializeComponent();
          }
     }
}
