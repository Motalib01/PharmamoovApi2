using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.Shop;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace PharmaMoov.API.Helpers
{
    public static class ModelBuilderExtensions
    {
        public static void SeedSuperAdmin(this ModelBuilder modelBuilder, string _hostUrl)
        {
            Guid AdminGuidID = Guid.NewGuid();
            DateTime CreatedDate = DateTime.Now;
            var DestinationFolderName = Path.Combine("Resources", "Icons");
            string hostURL = _hostUrl;
            var ServerPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName);
            string ShopCatDefaultLogo = hostURL + "resources/Icons/" + "Shop_Category_Default_Logo.jpg";

            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    AdminRecordID = 1,
                    AdminId = AdminGuidID,
                    ShopId = AdminGuidID,
                    //AccountType = AccountTypes.SUPERADMINUSER,
                    Email = "superadmin@PharmaMoov.com",
                    MobileNumber = "0123456789",
                    Password = "8KNY4oIOr8pgkLugTyGFB22tbsANYZc5B1iqE+bhyvk=",
                    FirstName = "Super Admin",
                    LastName = "Administrator",
                    UserTypeId = UserTypes.SUPERADMIN,
                    IsVerified = true,

                    LastEditedBy = AdminGuidID,
                    LastEditedDate = CreatedDate,
                    CreatedBy = AdminGuidID,
                    CreatedDate = CreatedDate,
                    IsEnabledBy = AdminGuidID,
                    IsEnabled = true,
                    DateEnabled = CreatedDate,
                    IsLocked = false,
                    LockedDateTime = null
                }
          );

            modelBuilder.Entity<UserType>().HasData(
               new UserType { UserTypeID = 1, Role = "Super Administrator", Description = "Has full access to backend", CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
               new UserType { UserTypeID = 2, Role = "Administrator", Description = "Has full access to backend", CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
               new UserType { UserTypeID = 3, Role = "Orderpicker", Description = "Has full access to orderpicker", CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null }
           );

            modelBuilder.Entity<ShopCategory>().HasData(
                 new ShopCategory { ShopCategoryID = 1, Name = "Boulangerie", Description = "Boulangerie Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 2, Name = "Boucherie", Description = "Boucherie Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 3, Name = "Chocolatier", Description = "Chocolatier Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 4, Name = "Fromager", Description = "Fromager Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 5, Name = "Glacier", Description = "Glacier Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 6, Name = "Patissier", Description = "Patissier Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 7, Name = "Poissonnier", Description = "Poissonnier Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 8, Name = "Sommelier", Description = "Sommelier Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                 new ShopCategory { ShopCategoryID = 9, Name = "Traiteur", Description = "Traiteur Shop Category", ImageUrl = ShopCatDefaultLogo, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null }
            );

            modelBuilder.Entity<OrderConfiguration>().HasData(
                new OrderConfiguration { ConfigDecValue = 3.0m, ConfigIntValue = 0,ConfigStrValue = "", ConfigType = OrderConfigType.COMMISSIONRATE, OrderConfigId = 1, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                new OrderConfiguration { ConfigDecValue = 50.0m, ConfigIntValue = 0, ConfigStrValue = "", ConfigType = OrderConfigType.MINORDER, OrderConfigId = 2, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                new OrderConfiguration { ConfigDecValue = 8.0m, ConfigIntValue = 0, ConfigStrValue = "", ConfigType = OrderConfigType.NORMALDELIVERYFEEINPARIS, OrderConfigId = 3, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                new OrderConfiguration { ConfigDecValue = 8.0m, ConfigIntValue = 0, ConfigStrValue = "", ConfigType = OrderConfigType.GREENDELIVERYINPARIS, OrderConfigId = 4, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                new OrderConfiguration { ConfigDecValue = 12.0m, ConfigIntValue = 0, ConfigStrValue = "", ConfigType = OrderConfigType.NORMALDELIVERYFEEOUTPARIS, OrderConfigId = 5, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null },
                new OrderConfiguration { ConfigDecValue = 12.0m, ConfigIntValue = 0, ConfigStrValue = "", ConfigType = OrderConfigType.GREENDELIVERYOUTPARIS, OrderConfigId = 6, CreatedBy = AdminGuidID, CreatedDate = CreatedDate, LastEditedBy = AdminGuidID, LastEditedDate = CreatedDate, IsEnabledBy = AdminGuidID, DateEnabled = CreatedDate, IsEnabled = true, IsLocked = false, LockedDateTime = null }
            );
        }
    }
}

