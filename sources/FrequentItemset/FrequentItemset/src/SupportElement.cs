namespace FrequentItemset
{
    public class SupportElement
    {
        private string m_label;
        private int m_count;

        public SupportElement(string label, int count)
        {
            m_label = label;
            m_count = count;
        }

        public string Label
        {
            get { return m_label; }
            set { m_label = value; }
        }

        public int Count
        {
            get { return m_count; }
            set { m_count = value; }
        }

        public SupportElement Clone()
        {
            SupportElement item = new SupportElement(m_label, m_count);
            return item;
        }

        public override string ToString()
        {
            return string.Format("Itemset: {0} - Support: {1}", m_label, m_count);
        }
    }
}