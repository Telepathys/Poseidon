
using System.Data;
using Dapper;


namespace Poseidon
{
    /// <summary>
    /// ㅋㅋㅋ
    /// </summary>
    public class TestRepository
    {
        public IList<TestEntity> findByAll (IDbConnection con)
        {
            IList<TestEntity> list;
            
            using (var conn = con)  
            {  
                var querySQL = @"SELECT * FROM test;";  
                list = conn.Query<TestEntity>(querySQL).ToList();  
            }
            return list;
        }
    }
}