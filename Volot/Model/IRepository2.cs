using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot.Model
{
    /// <summary>
    /// Repository description
    /// </summary>
    /// <typeparam name="ID">Repository id type</typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IRepository2<ID, T, T2>
    {
        /// <summary>
        /// Read data from repository
        /// </summary>
        /// <param name="id">Repository id</param>
        /// <returns>Data</returns>
        T Read(ID id);
        /// <summary>
        /// Create a new repository
        /// </summary>
        /// <param name="id">Repository id</param>
        /// <param name="item">Data for save into repository</param>
        void Create(ID id, T item);
        void Create2(ID id, T item, T2 item2);
        /// <summary>
        /// Remove repository
        /// </summary>
        /// <param name="id">Repository id</param>
        void Delete(ID id);
    }
}
