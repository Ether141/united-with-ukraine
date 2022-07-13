using System.Collections;
using UnityEngine;

public class GhostStates
{
    public class Patrol : IState
    {
        private readonly Transform transform;
        private readonly StraightPath path;
        private readonly float speed;

        private Vector2 targetPosition;

        public Patrol(Transform transform, float speed, StraightPath path)
        {
            this.transform = transform;
            this.speed = speed;
            this.path = path;
        }

        public void OnStateEnter() { }

        public void OnStateExit() { }

        public void Tick()
        {
            if (!path.IsOnPath(transform.position))
            {
                transform.position = Vector2.MoveTowards(transform.position, path.ClosestPoint(transform.position), Time.deltaTime * speed);
            }
            else
            {
                if (path.IsAtStart(transform.position))
                {
                    targetPosition = path.EndPosition;
                }
                else if (path.IsAtEnd(transform.position))
                {
                    targetPosition = path.StartPosition;
                }

                transform.position = Vector2.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
            }
        }
    }

    public class ChasePlayer : IState
    {
        private readonly Ghost controller;
        private readonly Transform transform;
        private readonly float speed;

        public ChasePlayer(Ghost ghost, float speed)
        {
            controller = ghost;
            transform = ghost.transform;
            this.speed = speed;
        }

        public void OnStateEnter() { }

        public void OnStateExit() { }

        public void Tick()
        {
            if (controller.CurrentTarget != null)
                transform.position = new Vector2(Vector2.MoveTowards(transform.position, controller.CurrentTarget.position, Time.deltaTime * speed).x, transform.position.y);
        }
    }

    public class WaitForHit : IState
    {
        private readonly Ghost controller;
        private readonly float minDelay;
        private readonly float maxDelay;
        private IEnumerator currentCor;

        public WaitForHit(Ghost controller, float minDelay, float maxDelay)
        {
            this.controller = controller;
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
        }

        public void OnStateEnter()
        {
            if (currentCor != null)
                controller.StopCoroutine(currentCor);
            currentCor = controller.WaitAndDo(Hit, Random.Range(minDelay, maxDelay));
        }

        public void OnStateExit() { }

        public void Tick() { }

        private void Hit() => controller.CallHit();
    }

    public class Hit : IState
    {
        private readonly Ghost controller;
        private readonly Transform transform;
        private Vector2 moveToPos;

        public Hit(Ghost controller)
        {
            this.controller = controller;
            transform = controller.transform;
        }

        public void OnStateEnter()
        {
            moveToPos = controller.CurrentTarget.position + (transform.right * controller.DistanceToAttack);
            controller.WaitAndDo(() => controller.ResetHitState(), 0.5f);
        }

        public void OnStateExit() { }

        public void Tick() 
        {
            transform.position = Vector2.Lerp(transform.position, moveToPos, Time.deltaTime * controller.AttackSpeed);
        }
    }

    public class TeleportBehindPlayer : IState
    {
        private readonly Transform transform;
        private readonly Ghost controller;

        public TeleportBehindPlayer(Ghost controller)
        {
            transform = controller.transform;
            this.controller = controller;
        }

        public void OnStateEnter() 
        {
            transform.position = controller.CurrentTarget.position + (-controller.CurrentTarget.right * controller.DistanceToAttack);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y == 0f ? 180f : 0f, transform.eulerAngles.z);
        }

        public void OnStateExit() { }

        public void Tick() { }
    }
}
