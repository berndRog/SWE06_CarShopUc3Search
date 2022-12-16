﻿using CarShop.DomainModel.Entities;
using CarShop.DomainModel.ValueObjects;
namespace CarShop; 
public interface IAppSearch {
   Result<IEnumerable<Car>> SearchCars(CarTs carTs);
}