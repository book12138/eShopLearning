using eShopLearning.Products.EFCoreRepositories.EFCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace eShopLearning.Products.EFCoreRepositories.Base
{
    /// <summary>
    /// 仓储基接口
    /// </summary>
    public interface IRepository<TEntity, TKey> where TEntity : Entity<TKey>
    {
        /// <summary>
        /// EF 上下文
        /// </summary>
        eShopProductDbContext Context { get;}

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="limit">每页显示数</param>
        /// <param name="total">总记录数</param>
        /// <param name="filterExpression">数据筛选表达式</param>
        /// <param name="orderByExpression">排序表达式</param>
        /// <param name="selectExpression"></param>
        /// <param name="isAsc">是否是正序排序</param>
        /// <returns></returns>
        IQueryable<TResult> GetPageData<TResult, TOrderKey>(int page, int limit, out int total, Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, TOrderKey>> orderByExpression, Expression<Func<TEntity, TResult>> selectExpression, bool isAsc);

        /// <summary>
        /// 分页获取数据（适用于滚动加载那种）
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="limit">每页显示数</param>
        /// <param name="filterExpression">数据筛选表达式</param>
        /// <param name="orderByExpression">排序表达式</param>
        /// <param name="selectExpression"></param>
        /// <param name="isAsc">是否是正序排序</param>
        /// <returns></returns>
        IQueryable<TResult> GetPageData<TResult, TOrderKey>(int page, int limit, Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, TOrderKey>> orderByExpression, Expression<Func<TEntity, TResult>> selectExpression, bool isAsc);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        Task AddAsync(TEntity entity);
        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task BatchAddAsync(IEnumerable<TEntity> entities);
        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task BatchAddAsync(TEntity[] entities);       
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        Task RemoveAsync(TEntity entity);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        Task RemoveAsync(TKey id);
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task BatchRemoveAsync(TKey[] ids);
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task BatchRemoveAsync(IEnumerable<TEntity> entities);
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="filterExpression">数据筛序 表达式树</param>
        /// <returns></returns>
        Task BatchRemoveAsync(Expression<Func<TEntity,bool>> filterExpression);
    }
}
