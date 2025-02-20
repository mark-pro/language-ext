﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

/// <summary>
/// Immutable stack
/// </summary>
/// <typeparam name="A">Stack element type</typeparam>
[Serializable]
[CollectionBuilder(typeof(Stack), nameof(Stack.createRange))]
public readonly struct Stck<A> : 
    IEnumerable<A>, 
    IEquatable<Stck<A>>,
    Monoid<Stck<A>>
{
    public static Stck<A> Empty { get; } = new(StckInternal<A>.Empty);

    readonly StckInternal<A> value;
    StckInternal<A> Value => value ?? StckInternal<A>.Empty;

    /// <summary>
    /// Default ctor
    /// </summary>
    internal Stck(StckInternal<A> value) =>
        this.value = value;

    /// <summary>
    /// Ctor that takes an initial state as an IEnumerable T
    /// </summary>
    public Stck(IEnumerable<A> initial)
    {
        var xs = initial.ToArray();
        value = xs.Length == 0 ? StckInternal<A>.Empty : new StckInternal<A>(xs);
    }

    /// <summary>
    /// Ctor that takes an initial state as an IEnumerable T
    /// </summary>
    public Stck(ReadOnlySpan<A> initial) =>
        value = initial.IsEmpty
                    ? StckInternal<A>.Empty
                    : new StckInternal<A>(initial);

    /// <summary>
    /// Reference version for use in pattern-matching
    /// </summary>
    /// <remarks>
    ///
    ///     Empty collection     = null
    ///     Singleton collection = A
    ///     More                 = (A, Seq〈A〉)   -- head and tail
    ///
    ///     var res = stack.Case switch
    ///     {
    ///       
    ///        (var x, var xs) => ...,
    ///        A value         => ...,
    ///        _               => ...
    ///     }
    /// 
    /// </remarks>
    [Pure]
    public object? Case =>
        IsEmpty
            ? null
            : toSeq(Value).Case;

    /// <summary>
    /// Reverses the order of the items in the stack
    /// </summary>
    /// <returns></returns>
    [Pure]
    public Stck<A> Reverse() =>
        new (Value.Reverse());

    /// <summary>
    /// Is the stack empty
    /// </summary>
    [Pure]
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => value?.IsEmpty ?? true;
    }

    /// <summary>
    /// Number of items in the stack
    /// </summary>
    [Pure]
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => value?.Count ?? 0;
    }

    /// <summary>
    /// Alias of Count
    /// </summary>
    [Pure]
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => value?.Count ?? 0;
    }
        
    /// <summary>
    /// Clear the stack (returns Empty)
    /// </summary>
    /// <returns>Stck.Empty of T</returns>
    [Pure]
    public Stck<A> Clear() =>
        Empty;

    /// <summary>
    /// Get enumerator
    /// </summary>
    /// <returns>IEnumerator of T</returns>
    [Pure]
    public IEnumerator<A> GetEnumerator() =>
        // ReSharper disable once NotDisposedResourceIsReturned
        AsIterable().GetEnumerator();

    /// <summary>
    /// Returns the stack as a Seq.  The first item in the sequence
    /// will be the item at the top of the stack.
    /// </summary>
    /// <returns>IEnumerable of T</returns>
    [Pure]
    public Seq<A> ToSeq() =>
        toSeq(Value);

    /// <summary>
    /// Format the collection as `[a, b, c, ...]`
    /// The elipsis is used for collections over 50 items
    /// To get a formatted string with all the items, use `ToFullString`
    /// or `ToFullArrayString`.
    /// </summary>
    [Pure]
    public override string ToString() =>
        CollectionFormat.ToShortArrayString(this, Count);

    /// <summary>
    /// Format the collection as `a, b, c, ...`
    /// </summary>
    [Pure]
    public string ToFullString(string separator = ", ") =>
        CollectionFormat.ToFullString(this, separator);

    /// <summary>
    /// Format the collection as `[a, b, c, ...]`
    /// </summary>
    [Pure]
    public string ToFullArrayString(string separator = ", ") =>
        CollectionFormat.ToFullArrayString(this, separator);

    /// <summary>
    /// Returns the stack as an IEnumerable.  The first item in the enumerable
    /// will be the item at the top of the stack.
    /// </summary>
    /// <returns>IEnumerable of T</returns>
    [Pure]
    public Iterable<A> AsIterable() =>
        Iterable.createRange(Value);

    /// <summary>
    /// Impure iteration of the bound value in the structure
    /// </summary>
    /// <returns>
    /// Returns the original unmodified structure
    /// </returns>
    public Stck<A> Do(Action<A> f)
    {
        this.Iter(f);
        return this;
    }

    /// <summary>
    /// Return the item on the top of the stack without affecting the stack itself
    /// NOTE: Will throw an ExpectedException if the stack is empty
    /// </summary>
    /// <exception cref="ExpectedException">Stack is empty</exception>
    /// <returns>Top item value</returns>
    [Pure]
    public A Peek() =>
        Value.Peek();

    /// <summary>
    /// Peek and match
    /// </summary>
    /// <param name="Some">Handler if there is a value on the top of the stack</param>
    /// <param name="None">Handler if the stack is empty</param>
    /// <returns>Untouched stack (this)</returns>
    [Pure]
    public Stck<A> Peek(Action<A> Some, Action None) =>
        new (Value.Peek(Some, None));

    /// <summary>
    /// Peek and match
    /// </summary>
    /// <typeparam name="R">Return type</typeparam>
    /// <param name="Some">Handler if there is a value on the top of the stack</param>
    /// <param name="None">Handler if the stack is empty</param>
    /// <returns>Return value from Some or None</returns>
    [Pure]
    public R Peek<R>(Func<A, R> Some, Func<R> None) =>
        Value.Peek(Some, None);

    /// <summary>
    /// Safely return the item on the top of the stack without affecting the stack itself
    /// </summary>
    /// <returns>Returns the top item value, or None</returns>
    [Pure]
    public Option<A> TryPeek() =>
        Value.TryPeek();

    /// <summary>
    /// Pop an item off the top of the stack
    /// NOTE: Will throw an ExpectedException if the stack is empty
    /// </summary>
    /// <exception cref="ExpectedException">Stack is empty</exception>
    /// <returns>Stack with the top item popped</returns>
    [Pure]
    public Stck<A> Pop() =>
        new (Value.Pop());

    /// <summary>
    /// Safe pop
    /// </summary>
    /// <returns>Tuple of popped stack and optional top-of-stack value</returns>
    [Pure]
    public (Stck<A> Stack, Option<A> Value) TryPop() =>
        Value.TryPop().MapFirst(x => new Stck<A>(x));

    /// <summary>
    /// Pop and match
    /// </summary>
    /// <param name="Some">Handler if there is a value on the top of the stack</param>
    /// <param name="None">Handler if the stack is empty</param>
    /// <returns>Popped stack</returns>
    [Pure]
    public Stck<A> Pop(Action<A> Some, Action None) =>
        new (Value.Pop(Some, None));

    /// <summary>
    /// Pop and match
    /// </summary>
    /// <typeparam name="R">Return type</typeparam>
    /// <param name="Some">Handler if there is a value on the top of the stack</param>
    /// <param name="None">Handler if the stack is empty</param>
    /// <returns>Return value from Some or None</returns>
    [Pure]
    public R Pop<R>(Func<Stck<A>, A, R> Some, Func<R> None) =>
        Value.Pop((s, t) => Some(new Stck<A>(s), t), None);

    /// <summary>
    /// Push an item onto the stack
    /// </summary>
    /// <param name="value">Item to push</param>
    /// <returns>New stack with the pushed item on top</returns>
    [Pure]
    public Stck<A> Push(A value) =>
        new (Value.Push(value));

    /// <summary>
    /// Get enumerator
    /// </summary>
    /// <returns>IEnumerator of T</returns>
    [Pure]
    IEnumerator IEnumerable.GetEnumerator() =>
        // ReSharper disable once NotDisposedResourceIsReturned
        AsIterable().GetEnumerator();
        
    /// <summary>
    /// Implicit conversion from an untyped empty list
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Stck<A>(SeqEmpty _) =>
        Empty;

    /// <summary>
    /// Append another stack to the top of this stack
    /// The rhs will be reversed and pushed onto 'this' stack.  That will
    /// maintain the order of the items in the resulting stack.  So the top
    /// of 'rhs' will be the top of the newly created stack.  'this' stack
    /// will be under the 'rhs' stack.
    /// </summary>
    [Pure]
    public static Stck<A> operator +(Stck<A> lhs, Stck<A> rhs) =>
        lhs.Combine(rhs);

    /// <summary>
    /// Append another stack to the top of this stack
    /// The rhs will be reversed and pushed onto 'this' stack.  That will
    /// maintain the order of the items in the resulting stack.  So the top
    /// of 'rhs' will be the top of the newly created stack.  'this' stack
    /// will be under the 'rhs' stack.
    /// </summary>
    /// <param name="rhs">Stack to append</param>
    /// <returns>Appended stacks</returns>
    [Pure]
    public Stck<A> Combine(Stck<A> rhs) =>
        new (Value.Combine(rhs.Value));

    /// <summary>
    /// Subtract one stack from another: lhs except rhs
    /// </summary>
    [Pure]
    public static Stck<A> operator -(Stck<A> lhs, Stck<A> rhs) =>
        lhs.Subtract(rhs);

    /// <summary>
    /// Append another stack to the top of this stack
    /// The rhs will be reversed and pushed onto 'this' stack.  That will
    /// maintain the order of the items in the resulting stack.  So the top
    /// of 'rhs' will be the top of the newly created stack.  'this' stack
    /// will be under the 'rhs' stack.
    /// </summary>
    /// <param name="rhs">Stack to append</param>
    /// <returns>Appended stacks</returns>
    [Pure]
    public Stck<A> Subtract(Stck<A> rhs) =>
        new (Enumerable.Except(this, rhs));

    [Pure]
    public static bool operator ==(Stck<A> lhs, Stck<A> rhs) =>
        lhs.Equals(rhs);

    [Pure]
    public static bool operator !=(Stck<A> lhs, Stck<A> rhs) =>
        !(lhs == rhs);

    [Pure]
    public override int GetHashCode() =>
        Value.GetHashCode();

    [Pure]
    public override bool Equals(object? obj) =>
        obj is Stck<A> @as && Equals(@as);

    [Pure]
    public bool Equals(Stck<A> other) =>
        GetHashCode() == other.GetHashCode() &&
        EqEnumerable<A>.Equals(Value, other.Value);
}
