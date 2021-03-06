using System;
using System.Collections;
using System.Collections.Generic;

namespace ReactiveData.ReactiveSequence
{
	public class ReactiveItemsBuilder<TElement> : IEnumerable<TElement>
	{
		private readonly List<IReactiveData<IEnumerable<TElement>>> _subsequences = new List<IReactiveData<IEnumerable<TElement>>>();
		private readonly List<TElement> _currItems = new List<TElement>();


		public void Add(TElement item)
		{
			_currItems.Add(item);
		}

		public void Add(IReactiveData<IEnumerable<TElement>> items)
		{
			FinishCurrItemsSubsequence();
			_subsequences.Add(items);
		}

		private void FinishCurrItemsSubsequence()
		{
			if (_currItems.Count > 0)
			{
				TElement[] currItemsArray = _currItems.ToArray();
				_subsequences.Add(new ReactiveConstant<IEnumerable<TElement>>(currItemsArray));
				_currItems.Clear();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<TElement> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		internal List<IReactiveData<IEnumerable<TElement>>> GetSubsequences()
		{
			FinishCurrItemsSubsequence();
			return _subsequences;
		}
	}
}