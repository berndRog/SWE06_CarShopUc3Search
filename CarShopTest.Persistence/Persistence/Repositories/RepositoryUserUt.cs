using CarShop;
using CarShop.DomainModel.Entities;
using CarShop.DomainModel.Enums;
using CarShop.Persistence;
using CarShop.Utilities;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace CarShopTest.Persistence.Repositories;
// Avoid parallel excution of tests with the same database
[Collection(nameof(SystemTestCollectionDefinition))]
public class RepositoryUserUt {

   private readonly Guid _id = new Guid("00010000-0000-0000-0000-000000000000");
   private const string _firstName = "Achim";
   private const string _lastName = "Arndt";
   private const string _email = "a.arndt@t-online.de";
   private const string _phone = "0123 456 7890";
   private const string _imagePath = @"C:\CarShop\images\image1.jpg";
   private const string _userName = "AchimA";
   private const string _salt = "iOQkTANBTh+MJZUtQRdEjZkCvukcokIBoU3Q1fUEFtY=";
   private const string _password = "geh1m_";
   private readonly Role _role = Role.User;
   private readonly User _user;

   private readonly IUnitOfWork _unitOfWork;

   public RepositoryUserUt() {
      var hashed = AppSecurity.HashPbkdf2(_password, _salt);
      _user = new User().Set(_id, _firstName, _lastName, _email, _phone, _imagePath,
                             _userName, hashed, _salt, _role);

      IServiceCollection serviceCollection = new ServiceCollection();
      serviceCollection.AddPersistenceTest();
      ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider()
         ?? throw new Exception("Failed to build ServiceProvider");

      _unitOfWork = serviceProvider.GetService<IUnitOfWork>()
         ?? throw new Exception("Error creating runtime object for IUnitOfWork");

      CDbContext dbContext = serviceProvider.GetService<CDbContext>()
         ?? throw new Exception("Failed to create runtime object for CDbContext");
      dbContext.Database.EnsureDeleted();
      dbContext.Database.EnsureCreated();
      dbContext.Dispose();

      // _unitOfWork.RepositoryUser.Add(NullUser.Instance);
      // _unitOfWork.SaveAllChanges(false);
      // _unitOfWork.Dispose();
   }

   #region User
   [Fact]
   public void FindByIdUt() {
      // Arrange
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.Add(_user);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryUser.FindById(_user.Id);
      // Assert
      actual.Should().BeEquivalentTo(_user);
      _unitOfWork.Dispose();
   }
   [Fact]
   public void FindByPredicateUt() {
      // Arrange
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryUser.Find(u => u.UserName == "AchimA");
      _unitOfWork.LogChangeTracker("FindByPredicate");
      // Assert
      actual.Should().BeEquivalentTo(_user);
      _unitOfWork.Dispose();
   }
   [Fact]
   public void SelectByPredicateUt() {
      // Arrange
      var seed = new CSeed();
      var expected = seed.Users.Where(u => u.Email.Contains("google.com"))
                                  .ToList();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryUser
                              .Select(u => u.Email.Contains("google.com"));
      _unitOfWork.LogChangeTracker("SelctByPredicate");
      // Assert
      actual.Count().Should().Be(2);
      actual.Should().BeEquivalentTo(expected);
      _unitOfWork.Dispose();
   }
   [Fact]
   public void SelectAllUt() {
      // Arrange
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryUser.SelectAll();
      // Assert
      actual.Count().Should().Be(6);
      actual.Should().BeEquivalentTo(seed.Users);
      _unitOfWork.Dispose();
   }
   [Fact]
   public void AddUt() {
      // Arrange
      var seed = new CSeed();
      // Act
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id);
      actual.Should().BeEquivalentTo(seed.User1);
      _unitOfWork.Dispose();
   }
   [Fact]
   public void UpdateUt() {
      // Arrange
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      seed.User1.Email = "achim.arndt@gmx.de";
      _unitOfWork.RepositoryUser.Update(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id);
      actual.Should().BeEquivalentTo(seed.User1);
   }
   [Fact]
   public void DeleteUt() {
      // Arrange
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      _unitOfWork.RepositoryUser.Remove(seed.User1);
      _unitOfWork.SaveAllChanges();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id);
      actual.Should().BeNull();
   }
   #endregion  

   #region User with Address
   [Fact]
   public void AddUser_And_AddAddressUt() {
      //-Arrange: add user ---------------------------------------
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();

      //-Act -----------------------------------------------------
      // C A S E (1): add address in domainmodel and add address
      // seed.User1.AddOrUpdateAddress(seed.Address1);
      // _unitOfWork.LogChangeTracker("act");
      // // add address and update user
      // _unitOfWork.RepositoryAddress.Add(seed.User1.Address!);
      // _unitOfWork.LogChangeTracker("after add address");
      // _unitOfWork.SaveAllChanges();      //  => E R R O R
      // _unitOfWork.Dispose();

      //-Act -----------------------------------------------------
      // C A S E (2): add address in domainmodel and update user
      // seed.User1.AddOrUpdateAddress(seed.Address1);
      // _unitOfWork.LogChangeTracker("act");
      // // add address and update user
      // _unitOfWork.RepositoryUser.Update(seed.User1);
      // _unitOfWork.LogChangeTracker("after update user");
      // _unitOfWork.SaveAllChanges();      //  => E R R O R
      // _unitOfWork.Dispose();

      //-Act -----------------------------------------------------
      // C A S E (3): add address in domainmodel and
      //              add address und update user
      // seed.User1.AddOrUpdateAddress(seed.Address1); // add address in domainmodel
      // _unitOfWork.LogChangeTracker("act");
      // // add address and update user
      // _unitOfWork.RepositoryAddress.Add(seed.User1.Address!);
      // _unitOfWork.LogChangeTracker("after add address");
      // _unitOfWork.RepositoryUser.Update(seed.User1);
      // _unitOfWork.LogChangeTracker("after update user");
      // _unitOfWork.SaveAllChanges();
      // _unitOfWork.Dispose();

      //-Act -----------------------------------------------------
      // C A S E (4): attach user and add address in domainmodel 
      //              add address and update user
      //_unitOfWork.RepositoryUser.Attach(seed.User1);
      //_unitOfWork.LogChangeTracker("act");
      //seed.User1.AddOrUpdateAddress(seed.Address1); // add address in domainmodel
      //_unitOfWork.RepositoryAddress.Add(seed.User1.Address!);
      //_unitOfWork.LogChangeTracker("after add address");
      //_unitOfWork.RepositoryUser.Update(seed.User1);
      //_unitOfWork.LogChangeTracker("after update user");
      //_unitOfWork.SaveAllChanges();
      //_unitOfWork.Dispose();

      //-Act -----------------------------------------------------
      // C A S E (5): attach user and add address in domainmodel 
      //              update user
      //_unitOfWork.RepositoryUser.Attach(seed.User1);
      //_unitOfWork.LogChangeTracker("act");
      //seed.User1.AddOrUpdateAddress(seed.Address1); // add address in domainmodel
      //_unitOfWork.RepositoryUser.Update(seed.User1);
      //_unitOfWork.LogChangeTracker("after update user");
      //_unitOfWork.SaveAllChanges();
      //_unitOfWork.Dispose();


      //-Act -----------------------------------------------------
      // C A S E (6): Select user from database   
      //              add address in domainmodel and
      //              update user with address      
      var user = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true) ??
         throw new Exception("select user is null");
      _unitOfWork.LogChangeTracker("act");
      user.AddOrUpdateAddress(seed.Address1);  // add address in domainmodel
      _unitOfWork.RepositoryAddress.Add(seed.Address1);     // not necessary
      _unitOfWork.LogChangeTracker("after add address");    // not necessary
      // update user with address
      _unitOfWork.RepositoryUser.Update(user);
      _unitOfWork.LogChangeTracker("after update user");
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();

      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true);  // with join
      _unitOfWork.LogChangeTracker("FindById join address");
      // ignore cyclic references 
      actual.Should().BeEquivalentTo(user, opt => opt.IgnoringCyclicReferences());
   }

   [Fact]
   public void AddUserWithAddressUt() {
      // Arrange
      var seed = new CSeed();
      // Act
      seed.User1.AddOrUpdateAddress(seed.Address1);
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true);  // with join
      _unitOfWork.LogChangeTracker("FindById join address");
      // ignore cyclic references 
      actual.Should().BeEquivalentTo(seed.User1, opt => opt.IgnoringCyclicReferences());
   }
   [Fact]
   public void AddUserWithAddress_And_UpdateUserAndAddressUt() {
      // Arrange
      var seed = new CSeed();
      seed.User1.AddOrUpdateAddress(seed.Address1);
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      seed.Address1.Street = "Hannoversche Str";
      seed.Address1.Number = "1a";
      seed.User1.AddOrUpdateAddress(seed.Address1);
      _unitOfWork.LogChangeTracker("act");
      _unitOfWork.RepositoryAddress.Update(seed.Address1);
      _unitOfWork.LogChangeTracker("after update address");
      _unitOfWork.RepositoryUser.Update(seed.User1);
      _unitOfWork.LogChangeTracker("after update user");
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true);
      actual.Should().BeEquivalentTo(seed.User1, opt => opt.IgnoringCyclicReferences());
   }
   [Fact]
   public void User_RemoveWithAddressUt() {
      // Arrange
      var seed = new CSeed();
      seed.User1.AddOrUpdateAddress(seed.Address1);
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      User user = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true) ??
         throw new Exception("user should not be null");
      _unitOfWork.RepositoryUser.Remove(user);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(user.Id, true);
      actual.Should().BeNull();
   }
   #endregion

   #region Users with Addresses and Cars
   [Fact]
   public void UserAddAddressAndAddCarUt() {
      
      // Arrange: add user
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();

      // Act: add address and update user
      _unitOfWork.LogChangeTracker("act");
      seed.User1.AddOrUpdateAddress(seed.Address1);
      _unitOfWork.RepositoryAddress.Add(seed.Address1);
      seed.User1.AddCar(seed.Car01);
      _unitOfWork.RepositoryCar.Add(seed.Car01);
      _unitOfWork.RepositoryUser.Update(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true, true);
      actual.Should().NotBeNull();
      actual.Should().BeEquivalentTo(seed.User1, opt => opt.IgnoringCyclicReferences());
      _unitOfWork.Dispose();
   }
   [Fact]
   public void UserWithAddressAndAddCarUt() {
      // Arrange: add user with address
      var seed = new CSeed();
      seed.User1.AddOrUpdateAddress(seed.Address1);
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act:  add car and update user
      User user = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true, true) ??
         throw new Exception("user should not be null");
      _unitOfWork.LogChangeTracker("FindById join");
      user.AddCar(seed.Car01);
      _unitOfWork.RepositoryCar.Add(seed.Car01);
      _unitOfWork.RepositoryUser.Update(user);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(user.Id, true, true);
      actual.Should().NotBeNull();
      actual.Should().BeEquivalentTo(user, opt => opt.IgnoringCyclicReferences());
      _unitOfWork.Dispose();
   }

   [Fact]
   public void FindbyIdUt() {
      // Arrange: add user with address and a car
      var seed = new CSeed();
      seed.User1.AddOrUpdateAddress(seed.Address1);
      seed.User1.AddCar(seed.Car01);
      _unitOfWork.RepositoryAddress.Add(seed.Address1);
      _unitOfWork.RepositoryCar.Add(seed.Car01);
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true, true);
      // Assert (omit nested reference car.User)
      actual.Should().NotBeNull();
      actual.Should().BeEquivalentTo(seed.User1, opt => opt.IgnoringCyclicReferences());
   }


   #endregion
   
}