﻿using System;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class Prelude
{
    /// <summary>
    /// Functor map operation
    /// </summary>
    /// <remarks>
    /// Unwraps the value within the functor, passes it to the map function `f` provided, and
    /// then takes the mapped value and wraps it back up into a new functor.
    /// </remarks>
    /// <param name="ma">Functor to map</param>
    /// <param name="f">Mapping function</param>
    /// <returns>Mapped functor</returns>
    public static ReaderT<Env, M, B> map<Env, M, A, B>(Func<A, B> f, K<ReaderT<Env, M>, A> ma) 
        where M : Monad<M>, Alternative<M> =>
        Functor.map(f, ma).As();
    
    /// <summary>
    /// Applicative action: runs the first applicative, ignores the result, and returns the second applicative
    /// </summary>
    public static ReaderT<Env, M, B> action<Env, M, A, B>(K<ReaderT<Env, M>, A> ma, K<ReaderT<Env, M>, B> mb) 
        where M : Monad<M>, Alternative<M> =>
        Applicative.action(ma, mb).As();

    /// <summary>
    /// Applicative functor apply operation
    /// </summary>
    /// <remarks>
    /// Unwraps the value within the `ma` applicative-functor, passes it to the unwrapped function(s) within `mf`, and
    /// then takes the resulting value and wraps it back up into a new applicative-functor.
    /// </remarks>
    /// <param name="ma">Value(s) applicative functor</param>
    /// <param name="mf">Mapping function(s)</param>
    /// <returns>Mapped applicative functor</returns>
    public static ReaderT<Env, M, B> apply<Env, M, A, B>(K<ReaderT<Env, M>, Func<A, B>> mf, K<ReaderT<Env, M>, A> ma) 
        where M : Monad<M>, Alternative<M> =>
        Applicative.apply(mf, ma).As();
}    
