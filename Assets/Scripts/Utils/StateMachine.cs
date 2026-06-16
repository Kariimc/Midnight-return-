using System;
using System.Collections.Generic;
using UnityEngine;

namespace MidnightReturn.Utils
{
    public abstract class State<T>
    {
        protected T Owner;
        public string Name { get; protected set; }

        protected State(T owner, string name) { Owner = owner; Name = name; }

        public virtual void OnEnter(State<T> previous) { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnFixedUpdate(float dt) { }
        public virtual void OnExit(State<T> next) { }
    }

    public class StateMachine<T>
    {
        private readonly Dictionary<string, State<T>> _states = new();
        private State<T> _current;
        private bool _locked;

        public string CurrentState => _current?.Name ?? "none";
        public bool Is(string name) => _current?.Name == name;

        public StateMachine<T> Add(State<T> state)
        {
            _states[state.Name] = state;
            return this;
        }

        public bool Transition(string name)
        {
            if (_locked || !_states.TryGetValue(name, out var next)) return false;
            if (next == _current) return false;

            var prev = _current;
            prev?.OnExit(next);
            _current = next;
            _current.OnEnter(prev);
            return true;
        }

        public void Update(float dt) => _current?.OnUpdate(dt);
        public void FixedUpdate(float dt) => _current?.OnFixedUpdate(dt);
        public void Lock()   => _locked = true;
        public void Unlock() => _locked = false;
    }
}
