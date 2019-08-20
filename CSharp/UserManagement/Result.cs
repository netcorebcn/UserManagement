﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.UserManagement
{
    public static class Result
    {
        public static IResult<S, E> Success<S, E>(S success)
        {
            return new SuccessResult<S, E>(success);
        }

        public static IResult<S, E> Error<S, E>(E error)
        {
            return new ErrorResult<S, E>(error);
        }

        public static IResult<S, E2> SelectError<S, E1, E2>(
            this IResult<S, E1> source,
            Func<E1, E2> selector)
        {
            return source.Accept(new SelectErrorResultVisitor<S, E1, E2>(selector));
        }

        private class SelectErrorResultVisitor<S, E1, E2> : IResultVisitor<S, E1, IResult<S, E2>>
        {
            private readonly Func<E1, E2> selector;

            public SelectErrorResultVisitor(Func<E1, E2> selector)
            {
                this.selector = selector;
            }

            public IResult<S, E2> VisitSuccess(S success)
            {
                return Success<S, E2>(success);
            }

            public IResult<S, E2> VisitError(E1 error)
            {
                return Error<S, E2>(selector(error));
            }
        }
    }
}
