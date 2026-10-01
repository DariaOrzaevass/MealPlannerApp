using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Abstractions;

//Unit of work interface for unit of work pattern,
//which is used to group several operations into a single transaction
public interface IUnitOfWork
{
    //Saves the changes to the database and returns how many rows were affected
    //Cancellation token allows the operation to be canceled
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
