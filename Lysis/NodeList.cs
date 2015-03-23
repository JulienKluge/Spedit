using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lysis
{
    public class NodeList
    {
        private DNode head_;

        public NodeList()
        {
            head_ = new DSentinel();
            head_.prev = head_;
            head_.next = head_;
        }

        public void insertBefore(DNode at, DNode node)
        {
            node.next = at;
            node.prev = at.prev;
            at.prev.next = node;
            at.prev = node;
        }
        public void insertAfter(DNode at, DNode node)
        {
            node.next = at;
            node.prev = at.prev;
            at.prev.next = node;
            at.prev = node;
        }
        public void add(DNode node)
        {
            insertBefore(head_, node);
        }
        public iterator begin()
        {
            return new iterator(head_.next);
        }
        public reverse_iterator rbegin()
        {
            return new reverse_iterator(head_.prev);
        }
        public void remove(iterator_base where)
        {
            DNode node = where.node;
            where.next();
            remove(node);
        }
        public void remove(DNode node)
        {
            node.prev.next = node.next;
            node.next.prev = node.prev;
            node.next = null;
            node.prev = null;
        }
        public void replace(DNode at, DNode with)
        {
            with.prev = at.prev;
            with.next = at.next;
            at.prev.next = with;
            at.next.prev = with;
            at.prev = null;
            at.next = null;
        }
        public void replace(iterator_base where, DNode with)
        {
            replace(where.node, with);
            where.node = with;
        }

        public DNode last
        {
            get { return head_.prev; }
        }
        public DNode first
        {
            get { return head_.next; }
        }

        public abstract class iterator_base
        {
            protected DNode node_;

            public iterator_base(DNode node)
            {
                node_ = node;
            }

            public bool more()
            {
                return node_.type != NodeType.Sentinel;
            }

            public abstract void next();

            public DNode node
            {
                get { return node_; }
                set { node_ = value; }
            }
        }

        public class iterator : iterator_base
        {
            public iterator(DNode node)
                : base(node)
            {
            }

            public override void next()
            {
                node_ = node_.next;
            }
        }

        public class reverse_iterator : iterator_base
        {
            public reverse_iterator(DNode node)
                : base(node)
            {
            }

            public override void next()
            {
                node_ = node_.prev;
            }
        }
    }
}
