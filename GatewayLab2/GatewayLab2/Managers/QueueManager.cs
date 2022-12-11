namespace GatewayLab2.Managers
{
    public class QueueManager
    {
        private List<Func<bool>> actions;

        public QueueManager()
        {
            actions = new List<Func<bool>>();
        }

        public void StartQueue()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    lock (actions)
                    {
                        List<Func<bool>> delAct = new List<Func<bool>>();

                        foreach (var act in actions)
                        {
                            var res = act();
                            if (res)
                            {
                                delAct.Add(act);
                            }
                                
                        }

                        actions = actions.Except(delAct).ToList();
                    }
                    

                    Thread.Sleep(10000);
                }
            });
        }

        public void AddNewAction(Func<bool> act)
        {
            lock (actions)
            {
                actions.Add(act);
            }
        }
    }
}
