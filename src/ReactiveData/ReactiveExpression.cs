using System;

namespace ReactiveData
{
    public interface IReactiveExpression
    {
        void OnDependencyChanged();
    }

    public class ReactiveExpression<TValue> : ReactiveChangeable<TValue>, IReactiveExpression where TValue : IEquatable<TValue>
    {
        private readonly Func<TValue> _expressionFunction;
        private TValue _value;
        private IReactiveData[] _dependencies = new IReactiveData[0];

        public ReactiveExpression(Func<TValue> expressionFunction)
        {
            _expressionFunction = expressionFunction;
        }

        public override event DataChangedEventHandler DataChanged {
            add {
                // If we're moving from lazy to reactive mode, because someone is now listening for changes, then compute our value
                // and update our dependencies, adding listeners for them
                if (!HaveSubscribers)
                    RecomputeDerivedValue();

                base.DataChanged += value;
            }

            remove {
                base.DataChanged -= value;

                // If we're moving from reactive mode to lazy mode, then forget the value (so it can be garbage collected) and stop
                // listening to our dependencies
                if (!HaveSubscribers) {
                    _value = default(TValue);

                    foreach (IReactiveData dependency in _dependencies)
                        dependency.RemoveExpressionDependingOnMe(this);
                    _dependencies = new IReactiveData[0];
                }
            }
        }

        public override TValue Value {
            get {
                // If no one is listening for changes to us, we're in lazy mode so just evaluate the function on demand
                if (!HaveSubscribers)
                    return _expressionFunction.Invoke();

                RunningDerivationsStack.Top?.AddDependency(this);

                return _value;
            }
        }

        public void OnDependencyChanged()
        {
            RecomputeDerivedValue();
            NotifyChanged();
        }

        private void RecomputeDerivedValue()
        {
            var runningDerivation = new RunningDerivation(_dependencies);
            RunningDerivation oldTopOfStack = RunningDerivationsStack.Top;
            RunningDerivationsStack.Top = runningDerivation;

            _value = _expressionFunction.Invoke();

            if (runningDerivation.DependenciesChanged)
                _dependencies = runningDerivation.UpdateExpressionDependencies(_dependencies, this);

            RunningDerivationsStack.Top = oldTopOfStack;
        }
    }
}
