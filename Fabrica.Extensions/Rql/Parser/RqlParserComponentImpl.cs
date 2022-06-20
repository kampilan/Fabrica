using System;
using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Rql.Builder;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Rql.Parser
{


    public class RqlParserComponentImpl: IRqlParserComponent
    {


        public RqlParserComponentImpl( ICorrelation correlation )
        {

            Correlation = correlation;

        }

        private ICorrelation Correlation { get; }



        public RqlFilterBuilder Parse(string rql)
        {

            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(rql), rql);


                // *****************************************************************
                if (string.IsNullOrWhiteSpace(rql))
                {
                    logger.Debug("Attempting to parse rql");
                    var builder = RqlFilterBuilder.Create();

                    return builder;
                }
                else
                {
                    logger.Debug("Attempting to parse rql");

                    var tree = RqlLanguageParser.ToFilter(rql);

                    var builder = new RqlFilterBuilder(tree);

                    return builder;
                }


            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, new { Rql = rql }, "Failed to parse RQL" );
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public RqlFilterBuilder ParseCriteria(string rql)
        {

            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(rql), rql);


                // *****************************************************************
                if (string.IsNullOrWhiteSpace(rql))
                {
                    logger.Debug("Attempting to parse rql");
                    var builder = RqlFilterBuilder.Create();

                    return builder;
                }
                else
                {
                    logger.Debug("Attempting to parse rql");

                    var tree = RqlLanguageParser.ToCriteria(rql);

                    var builder = new RqlFilterBuilder(tree);

                    return builder;
                }

            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, new { Rql = rql }, "Failed to parse RQL");
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public RqlFilterBuilder<TEntity> Parse<TEntity>(string rql) where TEntity : class, IModel
        {

            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(rql), rql);


                // *****************************************************************
                if ( string.IsNullOrWhiteSpace(rql) )
                {
                    logger.Debug("Attempting to parse rql");
                    var builder = RqlFilterBuilder<TEntity>.Create();

                    return builder;
                }
                else
                {
                    logger.Debug("Attempting to parse rql");

                    var tree = RqlLanguageParser.ToFilter(rql);
                    
                    var builder = new RqlFilterBuilder<TEntity>( tree );

                    return builder;
                }


            }
            catch (Exception cause)
            {
                logger.ErrorWithContext( cause, new { Rql = rql }, "Failed to parse RQL" );
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public RqlFilterBuilder<TEntity> ParseCriteria<TEntity>(string rql) where TEntity : class, IModel
        {

            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(rql), rql);


                // *****************************************************************
                if (string.IsNullOrWhiteSpace(rql))
                {
                    logger.Debug("Attempting to parse rql");
                    var builder = RqlFilterBuilder<TEntity>.Create();

                    return builder;
                }
                else
                {
                    logger.Debug("Attempting to parse rql");

                    var tree = RqlLanguageParser.ToCriteria(rql);

                    var builder = new RqlFilterBuilder<TEntity>( tree );

                    return builder;
                }

            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, new { Rql = rql }, "Failed to parse RQL");
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }

        }



    }

}
