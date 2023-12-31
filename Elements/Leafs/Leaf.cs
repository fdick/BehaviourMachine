﻿using System;
using BehaviourGraph.Trees;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph
{
    public class Leaf : ILeaf, IDisposable
    {
        public string FriendlyName { get; set; }
        public string Tag { get; set; }
        public GUID ID { get; }
        protected GameObject _gameObject;
        protected bool _isRunning;
        protected float _lastProcCD;

        public Leaf(string tag = null)
        {
            ID = GUID.Generate();
            FriendlyName = this.ToString();
            Tag = tag;
        }

        public Leaf(Action onEnter, Action onExit, string tag = null)
        {
            ID = GUID.Generate();
            FriendlyName = this.ToString();
            OnEnter += (c) => onEnter?.Invoke();
            OnExit += () => onExit?.Invoke();
            Tag = tag;
        }

        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }


        public virtual void InitLeaf()
        {
        }

        public virtual void EnterLeaf(Transition condData)
        {
            OnEnter?.Invoke(condData);
            _isRunning = true;
        }

        public virtual void ExitLeaf()
        {
            _lastProcCD = Time.time;
            _isRunning = false;
            OnExit?.Invoke();
        }

        public void SetGameobject(GameObject go)
        {
            _gameObject = go;
        }

        public void Dispose()
        {
            OnEnter = null;
            OnExit = null;
        }

        public bool CheckCD(float duration)
        {
            return !_isRunning && (Time.time >= _lastProcCD + duration || _lastProcCD == 0);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(FriendlyName)? this.GetType().ToString() : FriendlyName;
        }
    }
}