namespace CarShop; 
public interface IUnitOfWork: IDisposable {
   void Init();
   IRepositoryUser RepositoryUser { get; }
   IRepositoryAddress RepositoryAddress { get; }
   IRepositoryCar RepositoryCar { get; }
   bool SaveAllChanges();
   void LogChangeTracker(string text); 
}
