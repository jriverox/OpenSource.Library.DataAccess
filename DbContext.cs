using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

namespace OpenSource.Library.DataAccess
{
  public class DbContext
  {
    private int maxCommandTimeout;
    private Database database;
    private string profileName;
    private string logFileName;
    private DbConnection connection;
    private DbTransaction transaction;
    private ICollection<IDbConcurrency> entities;

    public event DbContext.ProfileChangeHandler ProfileChanged;

    public DbContext()
    {
      this.ChangeProfile(string.Empty, true);
    }

    public DbContext(string profileName)
    {
      this.ChangeProfile(profileName, true);
    }

    public Database Database
    {
      get
      {
        return this.database;
      }
    }

    public string ProfileName
    {
      get
      {
        return this.profileName;
      }
      set
      {
        if (!(this.profileName != value))
          return;
        this.ChangeProfile(value, false);
      }
    }

    public string LogFileName
    {
      get
      {
        return this.logFileName;
      }
      set
      {
        this.logFileName = value;
      }
    }

    public int MaxCommandTimeout
    {
      get
      {
        return this.maxCommandTimeout;
      }
      set
      {
        this.maxCommandTimeout = value;
      }
    }

    public DateTime ServerDate
    {
      get
      {
        try
        {
          return Convert.ToDateTime(this.ExecuteScalar(this.database.GetSqlStringCommand("SELECT GETDATE()")));
        }
        catch
        {
          return DateTime.Now;
        }
      }
    }

    private void ChangeProfile(string profileName, bool flagCreate)
    {
      this.profileName = profileName;
      if (flagCreate)
      {
        this.maxCommandTimeout = 120;
        this.logFileName = (string) null;
      }
      this.database = !(profileName == string.Empty) ? DatabaseFactory.CreateDatabase(profileName) : DatabaseFactory.CreateDatabase();
      if (this.ProfileChanged == null)
        return;
      this.ProfileChanged();
    }

    public void BeginTransaction(IsolationLevel isolationLevel)
    {
      if (this.transaction != null)
        return;
      try
      {
        this.connection = this.database.CreateConnection();
        this.connection.Open();
        this.transaction = this.connection.BeginTransaction(isolationLevel);
        this.entities = (ICollection<IDbConcurrency>) new List<IDbConcurrency>();
      }
      catch
      {
        this.ClearTransaction();
        throw;
      }
    }

    public void BeginTransaction()
    {
      if (this.transaction != null)
        return;
      try
      {
        this.connection = this.database.CreateConnection();
        this.connection.Open();
        this.transaction = this.connection.BeginTransaction();
        this.entities = (ICollection<IDbConcurrency>) new List<IDbConcurrency>();
      }
      catch
      {
        this.ClearTransaction();
        throw;
      }
    }

    public void CommitTransaction()
    {
      if (this.transaction == null)
        return;
      try
      {
        this.transaction.Commit();
        this.connection.Close();
        foreach (IDbConcurrency entity in (IEnumerable<IDbConcurrency>) this.entities)
          entity.UpdateVersion();
        this.ClearTransaction();
      }
      catch
      {
        this.ClearTransaction();
        throw;
      }
    }

    public void RollbackTransaction()
    {
      if (this.transaction == null)
        return;
      try
      {
        this.transaction.Rollback();
        this.connection.Close();
        this.ClearTransaction();
      }
      catch
      {
        this.ClearTransaction();
        throw;
      }
    }

    private void ClearTransaction()
    {
      this.entities = (ICollection<IDbConcurrency>) null;
      this.transaction = (DbTransaction) null;
      this.connection = (DbConnection) null;
    }

    public DbConnection CreateConnection()
    {
      return this.database.CreateConnection();
    }

    public DbConnection Connection
    {
      get
      {
        return this.connection;
      }
    }

    public DbTransaction Transaction
    {
      get
      {
        return this.transaction;
      }
    }

    public int ExecuteNonQuery(DbCommand command, IDbConcurrency entity = null)
    {
      int num;
      try
      {
        num = this.transaction != null ? this.database.ExecuteNonQuery(command, this.transaction) : this.database.ExecuteNonQuery(command);
      }
      catch (DbException ex)
      {
        this.WriteLog(command, ex);
        throw;
      }
      if (entity != null)
      {
        if (num == 0)
          throw new DBConcurrencyException();
        if (this.transaction == null)
          entity.UpdateVersion();
        else
          this.entities.Add(entity);
      }
      return num;
    }

    public DataSet ExecuteDataSet(DbCommand command)
    {
      try
      {
        return this.transaction == null ? this.database.ExecuteDataSet(command) : this.database.ExecuteDataSet(command, this.transaction);
      }
      catch (DbException ex)
      {
        this.WriteLog(command, ex);
        throw;
      }
    }

    public DataTable ExecuteDataTable(DbCommand command)
    {
      try
      {
        return this.transaction == null ? this.database.ExecuteDataSet(command).Tables[0] : this.database.ExecuteDataSet(command, this.transaction).Tables[0];
      }
      catch (DbException ex)
      {
        this.WriteLog(command, ex);
        throw;
      }
    }

    public IDataReader ExecuteReader(DbCommand command)
    {
      try
      {
        return this.transaction == null ? this.database.ExecuteReader(command) : this.database.ExecuteReader(command, this.transaction);
      }
      catch (DbException ex)
      {
        this.WriteLog(command, ex);
        throw;
      }
    }

    public object ExecuteScalar(DbCommand command)
    {
      try
      {
        return this.transaction == null ? this.database.ExecuteScalar(command) : this.database.ExecuteScalar(command, this.transaction);
      }
      catch (DbException ex)
      {
        this.WriteLog(command, ex);
        throw;
      }
    }

    private void WriteLog(DbCommand command, DbException exception)
    {
      if (this.logFileName == null)
        return;
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(this.logFileName, true))
        {
          streamWriter.WriteLine("---------------------------------------------------------------");
          streamWriter.WriteLine("Error OpenSource.Library.DataAccess");
          streamWriter.WriteLine("Command: " + command.CommandText);
          streamWriter.WriteLine("Date: " + (object) DateTime.Now);
          streamWriter.WriteLine("Source: " + exception.Source.ToString());
          streamWriter.WriteLine("Type: " + exception.GetType().ToString());
          streamWriter.WriteLine("ErrorCode: " + exception.ErrorCode.ToString());
          streamWriter.WriteLine("Message: " + exception.Message.ToString());
          foreach (DbParameter parameter in command.Parameters)
            streamWriter.WriteLine("Parameter: " + parameter.ParameterName + " = " + parameter.Value.ToString() + ", Type: " + parameter.DbType.ToString() + ", Direction: " + parameter.Direction.ToString() + ", Size: " + parameter.Size.ToString());
        }
      }
      catch
      {
      }
    }

    public delegate void ProfileChangeHandler();
  }
}
