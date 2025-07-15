using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Attachment;
using PharmaMoov.Models.Product;
using PharmaMoov.Models.User;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.Shop;
using PharmaMoov.Models.Campaign;
using PharmaMoov.Models;
using PharmaMoov.Models.Review;
using PharmaMoov.Models.Promo;
using PharmaMoov.Models.Prescription;
using PharmaMoov.Models.Patient;
using PharmaMoov.Models.DeliveryUser;

namespace PharmaMoov.API.DataAccessLayer
{
    public class APIDBContext : DbContext
    {
        public APIConfigurationManager MasterConf { get; set; }

        public APIDBContext(DbContextOptions<APIDBContext> options, APIConfigurationManager _acm) : base(options) { MasterConf = _acm; }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<DeliveryJob> DeliveryJobs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderConfiguration> OrderConfigurations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ExternalProduct> ExternalProducts { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Promo> Promos { get; set; }
        public DbSet<PrivacyPolicy> PrivacyPolicies { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<ShopCategory> ShopCategories { get; set; }
        public DbSet<ShopFAQ> ShopFAQs { get; set; }
        public DbSet<ShopRating> ShopRatings { get; set; }
        public DbSet<ShopRequest> ShopRequests { get; set; }
        public DbSet<ShopReviewRating> ShopReviewRatings { get; set; }
        public DbSet<ShopOpeningHour> ShopOpeningHours { get; set; }
        public DbSet<ShopDocument> ShopDocuments { get; set; }
        public DbSet<TermsAndCondition> TermsAndConditions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAddresses> UserAddresses { get; set; }
        public DbSet<UserLoginTransaction> UserLoginTransactions { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<UserShopFavorite> UserShopFavorites { get; set; }
        public DbSet<UserVerificationCode> UserVerificationCodes { get; set; }
        public DbSet<UserHelpRequest> UserHelpRequests { get; set; }
        public DbSet<UserGeneralConcern> UserGeneralConcerns { get; set; }
        public DbSet<ShopTermsAndCondition> ShopTermsAndConditions { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PrescriptionProduct> PrescriptionProducts { get; set; }
        public DbSet<DeliveryUserLocation> DeliveryUserLocation { get; set; }
        public DbSet<DeliveryUserOrder> DeliveryUserOrders { get; set; }
        public DbSet<MangoPayUser> MangoPayUsers { get; set; } 
        public DbSet<CardRegistration> CardRegistrations { get; set; }
        public DbSet<MangoPayShop> MangoPayShops { get; set; }
        public DbSet<MangoPayUserWallet> MangoPayUserWallets { get; set; }
        public DbSet<MangoPayCouriers> MangoPayCouriers { get; set; }
        public DbSet<MangoPayCourierWallet> MangoPayCourierWallets { get; set; }
        public DbSet<PaymentDelivery> PaymentDelivery { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeliveryJob>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>()
            .HasOne(x => x.DeliveryJob)
            .WithOne(x => x.Order)
            .HasForeignKey<DeliveryJob>(b => b.OrderId);
            modelBuilder.Entity<Payment>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>()
            .HasOne(x => x.Payment)
            .WithOne(x => x.Order)
            .HasForeignKey<Payment>(b => b.OrderId);

            modelBuilder.Entity<ShopRating>().HasNoKey().ToView("View_Shop_Ratings");

            modelBuilder.SeedSuperAdmin(MasterConf.HostURL);
        }
    }
}
