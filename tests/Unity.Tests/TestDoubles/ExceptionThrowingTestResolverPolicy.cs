﻿using System;
using Unity.Builder;
using Unity.Policy;

namespace Unity.Tests.v5.TestDoubles
{
    public class ExceptionThrowingTestResolverPolicy : IResolverPolicy
    {
        private Exception exceptionToThrow;

        public ExceptionThrowingTestResolverPolicy(Exception exceptionToThrow)
        {
            this.exceptionToThrow = exceptionToThrow;
        }

        public object Resolve(IBuilderContext context)
        {
            throw this.exceptionToThrow;
        }
    }
}
