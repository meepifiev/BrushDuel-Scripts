using System;
using System.Collections.Generic;
using _Project.Application.Network;
using _Project.Core.Drawing;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Application.Drawing
{
    public class DrawingSyncSender : IStartable, ITickable, IDisposable
    {
        private const float FlushIntervalSeconds = 0.1f;

        private readonly DrawingSessionService _session;
        private readonly IDrawingSyncChannel _channel;
        private readonly List<CanvasPoint> _pendingPoints = new List<CanvasPoint>();

        private StrokeStartData _pendingStart;
        private bool _hasPendingStart;
        private bool _strokeActive;
        private float _sinceFlush;

        public DrawingSyncSender(DrawingSessionService session, IDrawingSyncChannel channel)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public void Start()
        {
            _session.StrokePointsAppended += OnStrokePointsAppended;
            _session.CommandApplied += OnCommandApplied;
            _session.CommandCompleted += OnCommandCompleted;
            _session.DocumentChanged += OnDocumentChanged;
        }

        public void Tick()
        {
            _sinceFlush += Time.deltaTime;

            if (_sinceFlush < FlushIntervalSeconds)
            {
                return;
            }

            Flush();
        }

        public void Dispose()
        {
            _session.StrokePointsAppended -= OnStrokePointsAppended;
            _session.CommandApplied -= OnCommandApplied;
            _session.CommandCompleted -= OnCommandCompleted;
            _session.DocumentChanged -= OnDocumentChanged;
        }

        private void Flush()
        {
            _sinceFlush = 0f;

            if (_hasPendingStart)
            {
                _hasPendingStart = false;
                _channel.SendStrokeBegan(_pendingStart, _pendingPoints);
                _pendingPoints.Clear();
                return;
            }

            if (_pendingPoints.Count == 0)
            {
                return;
            }

            _channel.SendStrokeContinued(_pendingPoints);
            _pendingPoints.Clear();
        }

        private void OnStrokePointsAppended(StrokeCommand stroke, int firstNewPointIndex)
        {
            if (firstNewPointIndex == 0)
            {
                Flush();
                _pendingStart = new StrokeStartData(stroke.Brush, stroke.SizeIndex, stroke.Color);
                _hasPendingStart = true;
                _strokeActive = true;
            }

            for (int i = firstNewPointIndex; i < stroke.Points.Count; i++)
            {
                _pendingPoints.Add(stroke.Points[i]);
            }
        }

        private void OnCommandApplied(DrawingCommand command)
        {
            if (command is FillCommand fill == false)
            {
                return;
            }

            Flush();
            _channel.SendFill(fill.Point, fill.Color);
        }

        private void OnCommandCompleted()
        {
            if (_strokeActive == false)
            {
                return;
            }

            _strokeActive = false;
            Flush();
            _channel.SendStrokeEnded();
        }

        private void OnDocumentChanged()
        {
            _strokeActive = false;
            _pendingPoints.Clear();
            _hasPendingStart = false;
            _channel.SendUndo();
        }
    }
}
