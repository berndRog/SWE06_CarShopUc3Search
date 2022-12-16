using CarShop;
using CarShop.DomainModel.Entities;
using CarShop.Persistence;
using CarShop.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CarShopTest.Persistence.Repositories;

[Collection(nameof(SystemTestCollectionDefinition))]
public class RepositoryCarUt {

   private readonly IUnitOfWork _unitOfWork;

   public RepositoryCarUt() {
      IServiceCollection serviceCollection = new ServiceCollection();
      serviceCollection.AddPersistenceTest();
      ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider()
         ?? throw new Exception("Failed to build ServiceProvider");
      
      var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
      unitOfWork.Should().NotBeNull();
      _unitOfWork = unitOfWork!;

      var dbContext = serviceProvider.GetService<CDbContext>();
      dbContext.Should().NotBeNull();
      dbContext!.Database.EnsureDeleted();
      dbContext!.Database.EnsureCreated();
      dbContext!.Dispose();
   }

   [Fact]
   public void FindbyIdWithRepoCarUt() {
      // Arrange
      var seed = new CSeed();
      seed.User1.AddCar(seed.Car01);
      _unitOfWork.RepositoryCar.Add(seed.Car01);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.FindById(seed.Car01.Id);
      // Assert (omit nested reference car.User)
      actual.Should().NotBeNull();
      seed.Car01.Should().BeEquivalentTo(actual, opt => opt.IgnoringCyclicReferences());
   }
   [Fact]
   public void FindByPredicateUt() {
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryCar.AddRange(seed.OfferedCars);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.Find(c => c.Make == "Audi");
      // Assert
      actual.Should().NotBeNull();
      var expected = seed.Cars.FirstOrDefault(c => c.Make == "Audi");
      expected.Should().NotBeNull();
      expected.Should().BeEquivalentTo(actual, opt => opt.IgnoringCyclicReferences());
   }
   [Fact]
   public void SelectByPredicateUt() {
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryCar.AddRange(seed.OfferedCars); 
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar
                              .Select(c => c.Make == "Volkswagen" && c.Model == "Golf");
      // Assert
      actual.Count().Should().Be(5);
      var expected = seed.Cars.Where(c => c.Make == "Volkswagen" && c.Model == "Golf")
                              .ToList();
      expected.Count().Should().Be(5);
      expected.Should().BeEquivalentTo(actual, opt => opt.IgnoringCyclicReferences());      
   }
   [Fact]
   public void SelectAllUt() {
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryCar.AddRange(seed.OfferedCars);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.SelectAll();
      // Assert
      actual.Count().Should().Be(21);
//    actual.Should().BeEquivalentTo(seed.OfferedCars, opt => opt.Excluding(c => c!.User));
      actual.Should().BeEquivalentTo(seed.OfferedCars, opt => opt.IgnoringCyclicReferences());
   }   
   [Fact]
   public void AddUt() {
      // Arrange
      var seed = new CSeed();
      _unitOfWork.RepositoryUser.Add(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      _unitOfWork.RepositoryUser.Attach(seed.User1);
      seed.User1.AddCar(seed.Car01);
      _unitOfWork.RepositoryCar.Add(seed.Car01);
//    _unitOfWork.RepositoryUser.Update(seed.User1);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Assert
      var actual = _unitOfWork.RepositoryUser.FindById(seed.User1.Id, true, true);
      actual.Should().BeEquivalentTo(seed.User1, opt => opt.IgnoringCyclicReferences());
   }
   [Fact]
   public void CountAllCarsUt(){
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.Count();
      // Assert
      actual.Should().Be(21);
   }

   [Fact]
   public void CountCarsByMakeUt() {
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.Count("Volkswagen");
      // Assert
      actual.Should().Be(7);
   }
   [Fact]
   public void CountCarsByMakeAndModelUt() {
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.Count("Volkswagen","Golf");
      // Assert
      actual.Should().Be(5);
   }
   [Fact]
   public void SelectMakesUt() {
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.SelectMakes();
      // Assert
      var expected = new List<string> { "Audi", "BMW", "Ford", "Mercedes-Benz", "Opel", "Volkswagen"};
      actual.Should().HaveCount(6).And
                     .BeEquivalentTo(expected);
   }
   [Fact]
   public void SelectModelsUt() {
      // Arrange
      var seed = new CSeed();
      seed.InitCars();
      _unitOfWork.RepositoryUser.AddRange(seed.Users);
      _unitOfWork.SaveAllChanges();
      _unitOfWork.Dispose();
      // Act
      var actual = _unitOfWork.RepositoryCar.SelectModels("Volkswagen");
      // Assert
      var expected = new List<string> { "Golf", "Passat", "Polo"};
      actual.Should().HaveCount(3).And.BeEquivalentTo(expected);
   }
}