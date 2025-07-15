CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `Admins` (
    `AdminRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `AdminId` char(36) NOT NULL,
    `ShopId` char(36) NOT NULL,
    `UserTypeId` int NOT NULL,
    `AccountType` int NOT NULL,
    `ImageUrl` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `Password` longtext CHARACTER SET utf8mb4 NULL,
    `MobileNumber` longtext CHARACTER SET utf8mb4 NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NULL,
    `IsVerified` tinyint(1) NULL,
    CONSTRAINT `PK_Admins` PRIMARY KEY (`AdminRecordID`)
);

CREATE TABLE `Attachments` (
    `AttachmentId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `AttachmentName` longtext CHARACTER SET utf8mb4 NULL,
    `AttachmentFileName` longtext CHARACTER SET utf8mb4 NULL,
    `AttachmentUploadedFileName` longtext CHARACTER SET utf8mb4 NULL,
    `AttachmentType` longtext CHARACTER SET utf8mb4 NULL,
    `AttachmentServerPhysicalPath` longtext CHARACTER SET utf8mb4 NULL,
    `AttachmentExternalUrl` longtext CHARACTER SET utf8mb4 NULL,
    `UType` int NOT NULL,
    CONSTRAINT `PK_Attachments` PRIMARY KEY (`AttachmentId`)
);

CREATE TABLE `Campaigns` (
    `CampaignRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `Name` longtext CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `ImageUrl` longtext CHARACTER SET utf8mb4 NULL,
    `IsProductOfferBanner` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Campaigns` PRIMARY KEY (`CampaignRecordID`)
);

CREATE TABLE `CardRegistrations` (
    `RegistrationRecordID` int NOT NULL AUTO_INCREMENT,
    `Id` longtext CHARACTER SET utf8mb4 NULL,
    `Tag` longtext CHARACTER SET utf8mb4 NULL,
    `CreationDate` datetime(6) NOT NULL,
    `UserId` longtext CHARACTER SET utf8mb4 NULL,
    `AccessKey` longtext CHARACTER SET utf8mb4 NULL,
    `PreregistrationData` longtext CHARACTER SET utf8mb4 NULL,
    `CardRegistrationURL` longtext CHARACTER SET utf8mb4 NULL,
    `CardId` longtext CHARACTER SET utf8mb4 NULL,
    `RegistrationData` longtext CHARACTER SET utf8mb4 NULL,
    `ResultCode` longtext CHARACTER SET utf8mb4 NULL,
    `Currency` int NOT NULL,
    `Status` longtext CHARACTER SET utf8mb4 NULL,
    `CardType` int NOT NULL,
    `ApplicationUserID` char(36) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    `CreatedBy` char(36) NOT NULL,
    CONSTRAINT `PK_CardRegistrations` PRIMARY KEY (`RegistrationRecordID`)
);

CREATE TABLE `CartItems` (
    `CartItemId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    `PatientId` char(36) NOT NULL,
    `ProductRecordId` int NOT NULL,
    `ProductQuantity` decimal(4,1) NOT NULL,
    `PrescriptionRecordId` int NOT NULL,
    CONSTRAINT `PK_CartItems` PRIMARY KEY (`CartItemId`)
);

CREATE TABLE `DeliveryUserLocation` (
    `DeliveryUserLocationId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `DeliveryUserId` char(36) NOT NULL,
    `Latitude` decimal(10,8) NOT NULL,
    `Longitude` decimal(11,8) NOT NULL,
    `ReceiveOrder` tinyint(1) NOT NULL,
    CONSTRAINT `PK_DeliveryUserLocation` PRIMARY KEY (`DeliveryUserLocationId`)
);

CREATE TABLE `DeliveryUserOrders` (
    `DeliveryUserOrderId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OrderID` int NOT NULL,
    `DeliveryUserId` char(36) NOT NULL,
    `DeliveryStatus` int NOT NULL,
    CONSTRAINT `PK_DeliveryUserOrders` PRIMARY KEY (`DeliveryUserOrderId`)
);

CREATE TABLE `MangoPayCouriers` (
    `MangoPayCourierRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ID` longtext CHARACTER SET utf8mb4 NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NULL,
    `Address` longtext CHARACTER SET utf8mb4 NULL,
    `AddressObsolete` longtext CHARACTER SET utf8mb4 NULL,
    `Birthday` datetime(6) NULL,
    `Birthplace` longtext CHARACTER SET utf8mb4 NULL,
    `Nationality` longtext CHARACTER SET utf8mb4 NULL,
    `CountryOfResidence` longtext CHARACTER SET utf8mb4 NULL,
    `Occupation` longtext CHARACTER SET utf8mb4 NULL,
    `IncomeRange` int NULL,
    `ProofOfIdentity` longtext CHARACTER SET utf8mb4 NULL,
    `ProofOfAddress` longtext CHARACTER SET utf8mb4 NULL,
    `PharmaMUserId` char(36) NOT NULL,
    `KYCLevel` longtext CHARACTER SET utf8mb4 NULL,
    `WalletID` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_MangoPayCouriers` PRIMARY KEY (`MangoPayCourierRecordID`)
);

CREATE TABLE `MangoPayCourierWallets` (
    `MangoPayUserWalletRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OwnerID` longtext CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Balance` longtext CHARACTER SET utf8mb4 NULL,
    `Currency` longtext CHARACTER SET utf8mb4 NULL,
    `FundsType` longtext CHARACTER SET utf8mb4 NULL,
    `WalletID` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` char(36) NULL,
    `ShopID` char(36) NULL,
    `Tag` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_MangoPayCourierWallets` PRIMARY KEY (`MangoPayUserWalletRecordID`)
);

CREATE TABLE `MangoPayShops` (
    `MagoPayShopRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ProofOfRegistration` longtext CHARACTER SET utf8mb4 NULL,
    `Statute` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeCountryOfResidence` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeNationality` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeBirthday` datetime(6) NULL,
    `LegalRepresentativeEmail` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeAddressObsolete` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeAddress` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeLastName` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeFirstName` longtext CHARACTER SET utf8mb4 NULL,
    `HeadquartersAddressObsolete` longtext CHARACTER SET utf8mb4 NULL,
    `HeadquartersAddress` longtext CHARACTER SET utf8mb4 NULL,
    `LegalPersonType` longtext CHARACTER SET utf8mb4 NULL,
    `CompanyNumber` longtext CHARACTER SET utf8mb4 NULL,
    `Name` longtext CHARACTER SET utf8mb4 NULL,
    `ShareholderDeclaration` longtext CHARACTER SET utf8mb4 NULL,
    `LegalRepresentativeProofOfIdentity` longtext CHARACTER SET utf8mb4 NULL,
    `PharmaShopId` char(36) NOT NULL,
    `Id` longtext CHARACTER SET utf8mb4 NULL,
    `WalletID` longtext CHARACTER SET utf8mb4 NULL,
    `KYCLevels` int NULL,
    CONSTRAINT `PK_MangoPayShops` PRIMARY KEY (`MagoPayShopRecordID`)
);

CREATE TABLE `MangoPayUsers` (
    `MangoPayUserRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ID` longtext CHARACTER SET utf8mb4 NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NULL,
    `Address` longtext CHARACTER SET utf8mb4 NULL,
    `AddressObsolete` longtext CHARACTER SET utf8mb4 NULL,
    `Birthday` datetime(6) NULL,
    `Birthplace` longtext CHARACTER SET utf8mb4 NULL,
    `Nationality` longtext CHARACTER SET utf8mb4 NULL,
    `CountryOfResidence` longtext CHARACTER SET utf8mb4 NULL,
    `Occupation` longtext CHARACTER SET utf8mb4 NULL,
    `IncomeRange` int NULL,
    `ProofOfIdentity` longtext CHARACTER SET utf8mb4 NULL,
    `ProofOfAddress` longtext CHARACTER SET utf8mb4 NULL,
    `PharmaMUserId` char(36) NOT NULL,
    `KYCLevel` longtext CHARACTER SET utf8mb4 NULL,
    `WalletID` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_MangoPayUsers` PRIMARY KEY (`MangoPayUserRecordID`)
);

CREATE TABLE `MangoPayUserWallets` (
    `MangoPayUserWalletRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OwnerID` longtext CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Balance` longtext CHARACTER SET utf8mb4 NULL,
    `Currency` longtext CHARACTER SET utf8mb4 NULL,
    `FundsType` longtext CHARACTER SET utf8mb4 NULL,
    `WalletID` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` char(36) NULL,
    `ShopID` char(36) NULL,
    `Tag` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_MangoPayUserWallets` PRIMARY KEY (`MangoPayUserWalletRecordID`)
);

CREATE TABLE `OrderConfigurations` (
    `OrderConfigId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ConfigType` int NOT NULL,
    `ConfigIntValue` int NOT NULL,
    `ConfigDecValue` decimal(16,2) NOT NULL,
    `ConfigStrValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_OrderConfigurations` PRIMARY KEY (`OrderConfigId`)
);

CREATE TABLE `OrderItems` (
    `OrderItemRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OrderID` int NOT NULL,
    `ProductRecordId` int NOT NULL,
    `ProductQuantity` decimal(4,1) NOT NULL,
    `ProductPrice` decimal(16,2) NOT NULL,
    `ProductTaxValue` decimal(16,2) NOT NULL,
    `ProductTaxAmount` decimal(16,2) NOT NULL,
    `ProductPricePerKG` decimal(16,2) NULL,
    `ProductUnit` longtext CHARACTER SET utf8mb4 NULL,
    `SubTotal` decimal(16,2) NOT NULL,
    CONSTRAINT `PK_OrderItems` PRIMARY KEY (`OrderItemRecordID`)
);

CREATE TABLE `Orders` (
    `OrderID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OrderReferenceID` longtext CHARACTER SET utf8mb4 NULL,
    `OrderInvoiceNo` longtext CHARACTER SET utf8mb4 NULL,
    `ShopId` char(36) NOT NULL,
    `ShopName` longtext CHARACTER SET utf8mb4 NULL,
    `ShopAddress` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` char(36) NOT NULL,
    `PatientId` char(36) NOT NULL,
    `CustomerName` longtext CHARACTER SET utf8mb4 NULL,
    `OrderNote` longtext CHARACTER SET utf8mb4 NULL,
    `DeliveryAddressId` int NOT NULL,
    `DeliveryDate` datetime(6) NOT NULL,
    `DeliveryDay` longtext CHARACTER SET utf8mb4 NULL,
    `DeliveryTime` longtext CHARACTER SET utf8mb4 NULL,
    `PromoCode` longtext CHARACTER SET utf8mb4 NULL,
    `OrderSubTotalAmount` decimal(16,2) NOT NULL,
    `OrderVatAmount` decimal(16,2) NOT NULL,
    `OrderPromoAmount` decimal(16,2) NOT NULL,
    `OrderDeliveryFee` decimal(16,2) NOT NULL,
    `OrderGrossAmount` decimal(16,2) NOT NULL,
    `OrderProgressStatus` int NOT NULL,
    `OrderDeliveryType` int NOT NULL,
    `DeliveryMethod` int NOT NULL,
    `OrderPaymentType` int NOT NULL,
    `OrderPaymentStatus` int NOT NULL,
    `PackageType` int NOT NULL,
    `DeliveryJobId` int NULL,
    `PaymentId` int NULL,
    CONSTRAINT `PK_Orders` PRIMARY KEY (`OrderID`)
);

CREATE TABLE `Patients` (
    `PatientRecordId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `PatientId` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `MobileNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AddressName` longtext CHARACTER SET utf8mb4 NULL,
    `Street` longtext CHARACTER SET utf8mb4 NOT NULL,
    `POBox` longtext CHARACTER SET utf8mb4 NULL,
    `City` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AddressNote` longtext CHARACTER SET utf8mb4 NULL,
    `Latitude` decimal(10,8) NOT NULL,
    `Longitude` decimal(11,8) NOT NULL,
    CONSTRAINT `PK_Patients` PRIMARY KEY (`PatientRecordId`)
);

CREATE TABLE `PaymentDelivery` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OrderId` int NOT NULL,
    `AuthorId` longtext CHARACTER SET utf8mb4 NULL,
    `CreditedUserId` longtext CHARACTER SET utf8mb4 NULL,
    `DebitedFundsCurrency` int NOT NULL,
    `DebitedFunds` decimal(65,30) NOT NULL,
    `FeesCurrency` int NOT NULL,
    `Fees` decimal(65,30) NOT NULL,
    `DebitedWalletId` longtext CHARACTER SET utf8mb4 NULL,
    `CreditedWalletId` longtext CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL,
    CONSTRAINT `PK_PaymentDelivery` PRIMARY KEY (`Id`)
);

CREATE TABLE `PaymentTransactions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OrderId` int NOT NULL,
    `UserId` char(36) NOT NULL,
    `PatientId` char(36) NOT NULL,
    `PaymentStatus` int NOT NULL,
    `PaymentToken` longtext CHARACTER SET utf8mb4 NULL,
    `TransactionCode` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_PaymentTransactions` PRIMARY KEY (`Id`)
);

CREATE TABLE `PrescriptionProducts` (
    `PrescriptionProductRecordId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `PrescriptionRecordId` int NOT NULL,
    `ProductRecordId` int NOT NULL,
    `ProductQuantity` decimal(4,1) NOT NULL,
    `ProductPrice` decimal(16,2) NOT NULL,
    `ProductTaxValue` decimal(16,2) NOT NULL,
    `ProductTaxAmount` decimal(16,2) NOT NULL,
    `ProductUnit` longtext CHARACTER SET utf8mb4 NULL,
    `SubTotal` decimal(16,2) NOT NULL,
    `PrescriptionProductStatus` int NOT NULL,
    CONSTRAINT `PK_PrescriptionProducts` PRIMARY KEY (`PrescriptionProductRecordId`)
);

CREATE TABLE `Prescriptions` (
    `PrescriptionRecordId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `PrescriptionId` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    `ShopId` char(36) NOT NULL,
    `PatientId` char(36) NOT NULL,
    `MedicineDescription` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DoctorName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PrescriptionIcon` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PrescriptionStatus` int NOT NULL,
    CONSTRAINT `PK_Prescriptions` PRIMARY KEY (`PrescriptionRecordId`)
);

CREATE TABLE `PrivacyPolicies` (
    `PrivacyPolicyID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `PrivacyPolicyBody` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_PrivacyPolicies` PRIMARY KEY (`PrivacyPolicyID`)
);

CREATE TABLE `ProductCategories` (
    `ProductCategoryId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `ProductCategoryName` longtext CHARACTER SET utf8mb4 NULL,
    `ProductCategoryDesc` longtext CHARACTER SET utf8mb4 NULL,
    `ProductCategoryImage` longtext CHARACTER SET utf8mb4 NULL,
    `IsCategoryFeatured` tinyint(1) NOT NULL,
    CONSTRAINT `PK_ProductCategories` PRIMARY KEY (`ProductCategoryId`)
);

CREATE TABLE `Products` (
    `ProductRecordId` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ProductId` char(36) NOT NULL,
    `ShopId` char(36) NOT NULL,
    `ProductCategoryId` int NOT NULL,
    `ProductName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ProductDesc` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ProductIcon` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ProductPrice` decimal(16,2) NOT NULL,
    `ProductUnit` longtext CHARACTER SET utf8mb4 NULL,
    `ProductPricePerKG` decimal(16,2) NOT NULL,
    `ProductTaxValue` decimal(16,2) NOT NULL,
    `ProductSKUCode` longtext CHARACTER SET utf8mb4 NULL,
    `ProductSAPCode` longtext CHARACTER SET utf8mb4 NULL,
    `IsSale` tinyint(1) NOT NULL,
    `IsUnsold` tinyint(1) NOT NULL,
    `IsFragile` tinyint(1) NOT NULL,
    `ProductStatus` int NOT NULL,
    `IsProductFeature` tinyint(1) NOT NULL,
    `SalePrice` decimal(16,2) NOT NULL,
    CONSTRAINT `PK_Products` PRIMARY KEY (`ProductRecordId`)
);

CREATE TABLE `Promos` (
    `PromoRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `PromoName` longtext CHARACTER SET utf8mb4 NULL,
    `PromoCode` longtext CHARACTER SET utf8mb4 NULL,
    `PromoDescription` longtext CHARACTER SET utf8mb4 NULL,
    `PType` int NOT NULL,
    `PromoValue` decimal(16,2) NOT NULL,
    `ValidityPeriod` longtext CHARACTER SET utf8mb4 NULL,
    `ValidityDate` datetime(6) NOT NULL,
    `ValidFrom` datetime(6) NOT NULL,
    `ValidTo` datetime(6) NOT NULL,
    CONSTRAINT `PK_Promos` PRIMARY KEY (`PromoRecordID`)
);

CREATE TABLE `ShopCategories` (
    `ShopCategoryID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `Name` longtext CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `ImageUrl` longtext CHARACTER SET utf8mb4 NULL,
    `OrderSequence` int NOT NULL,
    CONSTRAINT `PK_ShopCategories` PRIMARY KEY (`ShopCategoryID`)
);

CREATE TABLE `ShopDocuments` (
    `ShopDocumentID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `FileName` longtext CHARACTER SET utf8mb4 NULL,
    `FileSize` longtext CHARACTER SET utf8mb4 NULL,
    `FileType` longtext CHARACTER SET utf8mb4 NULL,
    `FilePath` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_ShopDocuments` PRIMARY KEY (`ShopDocumentID`)
);

CREATE TABLE `ShopFAQs` (
    `ShopFAQID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `Question` longtext CHARACTER SET utf8mb4 NULL,
    `Answer` longtext CHARACTER SET utf8mb4 NULL,
    `Order` int NOT NULL,
    CONSTRAINT `PK_ShopFAQs` PRIMARY KEY (`ShopFAQID`)
);

CREATE TABLE `ShopOpeningHours` (
    `ShopOpeningHourID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `DayOfWeek` int NOT NULL,
    `StartTimeAM` longtext CHARACTER SET utf8mb4 NULL,
    `EndTimeAM` longtext CHARACTER SET utf8mb4 NULL,
    `StartTimePM` longtext CHARACTER SET utf8mb4 NULL,
    `EndTimePM` longtext CHARACTER SET utf8mb4 NULL,
    `StartTimeEvening` longtext CHARACTER SET utf8mb4 NULL,
    `EndTimeEvening` longtext CHARACTER SET utf8mb4 NULL,
    `NowOpen` tinyint(1) NOT NULL,
    `DeliveryType` int NOT NULL,
    CONSTRAINT `PK_ShopOpeningHours` PRIMARY KEY (`ShopOpeningHourID`)
);

CREATE TABLE `ShopRequests` (
    `ShopRequestID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `CompanyName` longtext CHARACTER SET utf8mb4 NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `MobileNumber` longtext CHARACTER SET utf8mb4 NULL,
    `Address` longtext CHARACTER SET utf8mb4 NULL,
    `SuiteAddress` longtext CHARACTER SET utf8mb4 NULL,
    `PostalCode` longtext CHARACTER SET utf8mb4 NULL,
    `City` longtext CHARACTER SET utf8mb4 NULL,
    `KbisNumber` int NOT NULL,
    `KbisDocument` longtext CHARACTER SET utf8mb4 NULL,
    `RegistrationStatus` int NOT NULL,
    CONSTRAINT `PK_ShopRequests` PRIMARY KEY (`ShopRequestID`)
);

CREATE TABLE `ShopReviewRatings` (
    `ShopReviewID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    `UserName` longtext CHARACTER SET utf8mb4 NULL,
    `ShopReview` longtext CHARACTER SET utf8mb4 NULL,
    `ShopRating` int NOT NULL,
    CONSTRAINT `PK_ShopReviewRatings` PRIMARY KEY (`ShopReviewID`)
);

CREATE TABLE `Shops` (
    `ShopRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `ShopCategoryId` int NOT NULL,
    `AccountType` int NOT NULL,
    `ShopName` longtext CHARACTER SET utf8mb4 NULL,
    `ShopLegalName` longtext CHARACTER SET utf8mb4 NULL,
    `ShopTags` longtext CHARACTER SET utf8mb4 NULL,
    `OwnerName` longtext CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `ShopIcon` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `Website` longtext CHARACTER SET utf8mb4 NULL,
    `TelephoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `MobileNumber` longtext CHARACTER SET utf8mb4 NULL,
    `TradeLicenseNo` longtext CHARACTER SET utf8mb4 NULL,
    `VATNumber` longtext CHARACTER SET utf8mb4 NULL,
    `Address` longtext CHARACTER SET utf8mb4 NULL,
    `SuiteAddress` longtext CHARACTER SET utf8mb4 NULL,
    `HeadquarterAddress` longtext CHARACTER SET utf8mb4 NULL,
    `PostalCode` longtext CHARACTER SET utf8mb4 NULL,
    `City` longtext CHARACTER SET utf8mb4 NULL,
    `Latitude` decimal(10,8) NOT NULL,
    `Longitude` decimal(11,8) NOT NULL,
    `HasOffers` tinyint(1) NULL,
    `DeliveryMethod` int NOT NULL,
    `PreparationTimeForDelivery` longtext CHARACTER SET utf8mb4 NULL,
    `PreparationTime` longtext CHARACTER SET utf8mb4 NULL,
    `DeliveryCommission` decimal(16,2) NOT NULL,
    `PickupCommission` decimal(16,2) NOT NULL,
    `ShopBanner` longtext CHARACTER SET utf8mb4 NULL,
    `OwnerFirstName` longtext CHARACTER SET utf8mb4 NULL,
    `OwnerLastName` longtext CHARACTER SET utf8mb4 NULL,
    `KbisNumber` int NOT NULL,
    `RegistrationStatus` int NOT NULL,
    `IsPopularPharmacy` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Shops` PRIMARY KEY (`ShopRecordID`)
);

CREATE TABLE `ShopTermsAndConditions` (
    `ShopTermsAndConditionID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopTermsAndConditionBody` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_ShopTermsAndConditions` PRIMARY KEY (`ShopTermsAndConditionID`)
);

CREATE TABLE `TermsAndConditions` (
    `TermsAndConditionID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `TermsAndConditionBody` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_TermsAndConditions` PRIMARY KEY (`TermsAndConditionID`)
);

CREATE TABLE `UserAddresses` (
    `UserAddressID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `UserId` char(36) NOT NULL,
    `AddressName` longtext CHARACTER SET utf8mb4 NULL,
    `Street` longtext CHARACTER SET utf8mb4 NULL,
    `City` longtext CHARACTER SET utf8mb4 NULL,
    `Area` longtext CHARACTER SET utf8mb4 NULL,
    `Building` longtext CHARACTER SET utf8mb4 NULL,
    `AddressNote` longtext CHARACTER SET utf8mb4 NULL,
    `Latitude` decimal(10,8) NOT NULL,
    `Longitude` decimal(11,8) NOT NULL,
    `IsCurrentAddress` tinyint(1) NOT NULL,
    `PostalCode` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_UserAddresses` PRIMARY KEY (`UserAddressID`)
);

CREATE TABLE `UserDevices` (
    `UserDeviceRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `UserId` char(36) NOT NULL,
    `DeviceFCMToken` longtext CHARACTER SET utf8mb4 NULL,
    `DeviceType` int NOT NULL,
    CONSTRAINT `PK_UserDevices` PRIMARY KEY (`UserDeviceRecordID`)
);

CREATE TABLE `UserGeneralConcerns` (
    `UserConcernRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `UserId` char(36) NOT NULL,
    `Subject` longtext CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_UserGeneralConcerns` PRIMARY KEY (`UserConcernRecordID`)
);

CREATE TABLE `UserHelpRequests` (
    `UserHelpRequestRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `UserId` char(36) NOT NULL,
    `OrderId` int NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `RequestStatus` int NOT NULL,
    CONSTRAINT `PK_UserHelpRequests` PRIMARY KEY (`UserHelpRequestRecordID`)
);

CREATE TABLE `UserLoginTransactions` (
    `UserLoginRecordID` int NOT NULL AUTO_INCREMENT,
    `UserId` char(36) NOT NULL,
    `AccountType` int NOT NULL,
    `Device` int NOT NULL,
    `Token` longtext CHARACTER SET utf8mb4 NULL,
    `TokenExpiration` datetime(6) NOT NULL,
    `RefreshToken` longtext CHARACTER SET utf8mb4 NULL,
    `RefreshTokenExpiration` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_UserLoginTransactions` PRIMARY KEY (`UserLoginRecordID`)
);

CREATE TABLE `Users` (
    `UserRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `UserId` char(36) NOT NULL,
    `AccountType` int NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `Password` longtext CHARACTER SET utf8mb4 NULL,
    `MobileNumber` longtext CHARACTER SET utf8mb4 NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NULL,
    `Username` longtext CHARACTER SET utf8mb4 NULL,
    `DateOfBirth` datetime(6) NULL,
    `Gender` int NOT NULL,
    `ImageUrl` longtext CHARACTER SET utf8mb4 NULL,
    `RegistrationPlatform` int NOT NULL,
    `RegistrationCode` longtext CHARACTER SET utf8mb4 NULL,
    `ProfessionalID` longtext CHARACTER SET utf8mb4 NULL,
    `KBIS` longtext CHARACTER SET utf8mb4 NULL,
    `UserFieldID` longtext CHARACTER SET utf8mb4 NULL,
    `HealthNumber` longtext CHARACTER SET utf8mb4 NULL,
    `MethodDelivery` int NOT NULL,
    `ForgotPasswordCode` longtext CHARACTER SET utf8mb4 NULL,
    `IsDecline` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`UserRecordID`)
);

CREATE TABLE `UserShopFavorites` (
    `FavoriteRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `ShopId` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    CONSTRAINT `PK_UserShopFavorites` PRIMARY KEY (`FavoriteRecordID`)
);

CREATE TABLE `UserType` (
    `UserTypeID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `Role` longtext CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_UserType` PRIMARY KEY (`UserTypeID`)
);

CREATE TABLE `UserVerificationCodes` (
    `UserCodeRecordID` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `UserId` char(36) NOT NULL,
    `Device` int NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `MobileNumber` longtext CHARACTER SET utf8mb4 NULL,
    `VerificationType` int NOT NULL,
    `VerificationCode` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_UserVerificationCodes` PRIMARY KEY (`UserCodeRecordID`)
);

CREATE TABLE `DeliveryJobs` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OrderId` int NOT NULL,
    `AssignmentCode` longtext CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL,
    `CreateParam` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_DeliveryJobs` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DeliveryJobs_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`OrderID`) ON DELETE CASCADE
);

CREATE TABLE `Payments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastEditedBy` char(36) NULL,
    `LastEditedDate` datetime(6) NULL,
    `CreatedBy` char(36) NULL,
    `CreatedDate` datetime(6) NULL,
    `IsEnabled` tinyint(1) NULL,
    `IsEnabledBy` char(36) NULL,
    `DateEnabled` datetime(6) NULL,
    `IsLocked` tinyint(1) NULL,
    `LockedDateTime` datetime(6) NULL,
    `OrderId` int NOT NULL,
    `Status` int NOT NULL,
    `StatusMessage` longtext CHARACTER SET utf8mb4 NULL,
    `Tag` longtext CHARACTER SET utf8mb4 NULL,
    `AuthorId` longtext CHARACTER SET utf8mb4 NULL,
    `DebitedFundsCurrency` longtext CHARACTER SET utf8mb4 NULL,
    `DebitedFundsAmount` bigint NOT NULL,
    `FeesCurrency` longtext CHARACTER SET utf8mb4 NULL,
    `FeesAmount` bigint NOT NULL,
    `CreditedWalletId` longtext CHARACTER SET utf8mb4 NULL,
    `ReturnURL` longtext CHARACTER SET utf8mb4 NULL,
    `Culture` longtext CHARACTER SET utf8mb4 NULL,
    `CardType` longtext CHARACTER SET utf8mb4 NULL,
    `SecureMode` longtext CHARACTER SET utf8mb4 NULL,
    `CreditedUserId` longtext CHARACTER SET utf8mb4 NULL,
    `StatementDescriptor` longtext CHARACTER SET utf8mb4 NULL,
    `CreatePayload` longtext CHARACTER SET utf8mb4 NULL,
    `LastPayloadUpdate` longtext CHARACTER SET utf8mb4 NULL,
    `LastPayloadUpdateDate` datetime(6) NULL,
    `CardId` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Payments_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`OrderID`) ON DELETE CASCADE
);

INSERT INTO `Admins` (`AdminRecordID`, `AccountType`, `AdminId`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Email`, `FirstName`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `IsVerified`, `LastEditedBy`, `LastEditedDate`, `LastName`, `LockedDateTime`, `MobileNumber`, `Password`, `ShopId`, `UserTypeId`)
VALUES (1, 0, '49192356-e11f-4915-9d08-4008cf7a2367', '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'superadmin@PharmaMoov.com', 'Super Admin', NULL, TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', 'Administrator', NULL, '0123456789', '8KNY4oIOr8pgkLugTyGFB22tbsANYZc5B1iqE+bhyvk=', '49192356-e11f-4915-9d08-4008cf7a2367', 1);

INSERT INTO `OrderConfigurations` (`OrderConfigId`, `ConfigDecValue`, `ConfigIntValue`, `ConfigStrValue`, `ConfigType`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`)
VALUES (5, 12.0, 0, '', 10, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL);
INSERT INTO `OrderConfigurations` (`OrderConfigId`, `ConfigDecValue`, `ConfigIntValue`, `ConfigStrValue`, `ConfigType`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`)
VALUES (4, 8.0, 0, '', 11, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL);
INSERT INTO `OrderConfigurations` (`OrderConfigId`, `ConfigDecValue`, `ConfigIntValue`, `ConfigStrValue`, `ConfigType`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`)
VALUES (3, 8.0, 0, '', 9, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL);
INSERT INTO `OrderConfigurations` (`OrderConfigId`, `ConfigDecValue`, `ConfigIntValue`, `ConfigStrValue`, `ConfigType`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`)
VALUES (2, 50.0, 0, '', 2, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL);
INSERT INTO `OrderConfigurations` (`OrderConfigId`, `ConfigDecValue`, `ConfigIntValue`, `ConfigStrValue`, `ConfigType`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`)
VALUES (6, 12.0, 0, '', 12, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL);
INSERT INTO `OrderConfigurations` (`OrderConfigId`, `ConfigDecValue`, `ConfigIntValue`, `ConfigStrValue`, `ConfigType`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`)
VALUES (1, 3.0, 0, '', 8, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL);

INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (8, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Sommelier Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Sommelier', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (1, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Boulangerie Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Boulangerie', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (2, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Boucherie Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Boucherie', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (3, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Chocolatier Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Chocolatier', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (4, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Fromager Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Fromager', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (5, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Glacier Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Glacier', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (6, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Patissier Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Patissier', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (7, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Poissonnier Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Poissonnier', 0);
INSERT INTO `ShopCategories` (`ShopCategoryID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `ImageUrl`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Name`, `OrderSequence`)
VALUES (9, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Traiteur Shop Category', 'http://localhost:63531/resources/Icons/Shop_Category_Default_Logo.jpg', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Traiteur', 0);

INSERT INTO `UserType` (`UserTypeID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Role`)
VALUES (3, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Has full access to orderpicker', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Orderpicker');
INSERT INTO `UserType` (`UserTypeID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Role`)
VALUES (2, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Has full access to backend', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Administrator');
INSERT INTO `UserType` (`UserTypeID`, `CreatedBy`, `CreatedDate`, `DateEnabled`, `Description`, `IsEnabled`, `IsEnabledBy`, `IsLocked`, `LastEditedBy`, `LastEditedDate`, `LockedDateTime`, `Role`)
VALUES (1, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', '2025-01-19 15:29:13.522374', 'Has full access to backend', TRUE, '49192356-e11f-4915-9d08-4008cf7a2367', FALSE, '49192356-e11f-4915-9d08-4008cf7a2367', '2025-01-19 15:29:13.522374', NULL, 'Super Administrator');

CREATE UNIQUE INDEX `IX_DeliveryJobs_OrderId` ON `DeliveryJobs` (`OrderId`);

CREATE UNIQUE INDEX `IX_Payments_OrderId` ON `Payments` (`OrderId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250119142913_firstMigration', '3.1.8');

