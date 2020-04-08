﻿using System.Text.RegularExpressions;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("NameFix", "Visagalis", "1.0.0")]
    [Description("Removes advertisements from player names when they login.")]

    class NameFix : CovalencePlugin
    {
        void OnUserConnected(IPlayer player)
        {
			string pattern = "[A-Za-z0-9-А-Яа-я]+\\.(com|lt|net|org|gg|ru|рф|int|info|ru.com|ru.net|com.ru|net.ru|рус|org.ru|moscow|biz|орг|москва|msk.ru|su|msk.su|md|tj|kz|tm|pw|travel|name|de|eu|eu.com|com.de|me|org.lv|pl|nl|at|co.at|be|wien|info.pl|cz|ch|com.pl|or.at|net.pl|org.pl|hamburg|cologne|koeln|berlin|de.com|es|biz.pl|bayern|scot|edu|edu.pl|com.es|nom.es|nom|nom.pl|brussels|org.es|gb|gb.net|shop|shop.pl|waw|waw.pl|wales|vlaanderen|gr.com|hu|hu.net|si|se|se.net|cymru|melbourne|im|sk|lat|gent|co.uk|uk|com.im|co.im|co|org.uk|me.uk|ist|saarland|org.im|istanbul|uk.net|uk.com|li|lu|gr|london|eu.com|lv|ro|com.ro|fi|net.fv|fv|com.lv|net.lv|as|asia|ind.in|net.ph|org.ph|io|jp|qa|ae.org|ae|ph|ind|af|jp.net|sa.com|sa|tl|tw|tv|tokyo|jpn.com|jpn|net.af|com.af|nagoya|org.af|com.tw|cn|cn.com|cx|la|club|club.tw|idv.tw|idv|yokohama|ebiz|ebiz.tw|mn|christmas|in|game|game.tw|to|com.my|co.in|in.net|net.in|net.my|org.my|ist|istanbul|pk|org.in|in.net|ph|com.ph|firm|firm.in|gen|gen.in|us|us.com|net.ec|ec|info.ec|co.lc|lc|com.lc|net.lc|org.lc|pro|pro.ec|med|med.ec|la|us.org|ag|gl|mx|com.mx|fin|fin.ec|co.ag|gl|mx|com.mx|pe|co.gl|com.gl|com.ag|net.ag|org.ag|net.gl|org.gl|net.pe|com.pe|gs|org.pe|nom|nom.ag|gy|sr|sx|bz|br|br.com|co.gy|co.bz|com.gy|vc|com.vc|net.vc|net.gy|hn|net.bz|com.bz|org.bz|com.hn|org.vc|co.ve|ve|net.hn|quebec|cl|org.hn|com.ve|ht|vegas|com.co|nyc|co.com|com.ht|us.com|miami|net.ht|org.ht|nom.co|nom|net.co|ec|info.ht|us.org|lc|com.ec|ac|as|mu|com.mu|tk|ws|net.mu|cc|cd|nf|org.mu|za|za.com|co.za|org.za|net.za|com.nf|net.nf|co.cm|cm|com.cm|org.nf|web|web.za|net.cm|ps|nu|net.so|nz|fm|irish|co.nz|radio|radio.fm|gg|net.nz|ml|com.ki|net.ki|ki|cf|org.nz|sb|com.sb|net.sb|tv|mg|srl|fm|sc|org.sb|biz.ki|org.ki|je|info.ki|net.sc|com.sc|durban|joburg|cc|capetown|sh|org.sc|ly|com.ly|ms|so|st|xyz|north-kazakhstan.su|nov|nov.su|ru.com|ru.net|com.ru|net.ru|org.ru|pp|pp.ru|msk.ru|msk|msk.su|spb|spb.ru|spb.su|tselinograd.su|ashgabad.su|abkhazia.su|adygeya.ru|adygeya.su|arkhangelsk.su|azerbaijan.su|balashov.su|bashkiria.ru|bashkiria.su|bir|bir.ru|bryansk.su|obninsk.su|penza.su|pokrovsk.su|pyatigorsk.ru|sochi.su|tashkent.su|termez.su|togliatti.su|troitsk.su|tula.su|tuva.su|vladikavkaz.su|vladikavkaz.ru|vladimir.ru|vladimir.su|spb.su|tatar|com.ua|kiev.ua|co.ua|biz.ua|pp.ua|am|co.am|com.am|net.am|org.am|net.am|radio.am|armenia.su|georgia.su|com.kz|bryansk.su|bukhara.su|cbg|cbg.ru|dagestan.su|dagestan.ru|grozny.su|grozny.ru|ivanovo.su|kalmykia.ru|kalmykia.su|kaluga.su|karacol.su|karelia.su|khakassia.su|krasnodar.su|kurgan.su|lenug.su|com.ua|ru.com|ялта.рф|тарханкут.рф|симфи.рф|севастополь.рф|ореанда.рф|массандра.рф|коктебель.рф|казантип.рф|инкерман.рф|евпатория.рф|донузлав.рф|балаклава.рф|vologda.su|org.kz|aktyubinsk.su|chimkent.su|east-kazakhstan.su|jambyl.su|karaganda.su|kustanal.ru|mangyshlak.su|kiev.ua|co.ua|biz.ua|radio.am|nov.ru|navoi.sk|nalchik.su|nalchik.ru|mystis.ru|murmansk.su|mordovia.su|mordovia.ru|marine.ru|tel|aero|mobi|xxx|aq|ax|az|bb|ba|be|bg|bi|bj|bh|bo|bs|bt|ca|cat|cd|cf|cg|ch|ci|ck|co.ck|co.ao|co.bw|co.id|id|co.fk|co.il|co.in|il|ke|ls|co.ls|mz|no|co.mz|co.no|th|tz|co.th|co.tz|uz|uk|za|zm|zw|co.uz|co.uk|co.za|co.zm|co.zw|ar|au|cy|eg|et|fj|gt|gu|gn|gh|hk|jm|kh|kw|lb|lr|com.ai|com.ar|com.au|com.bd|com.bn|com.br|com.cn|com.cy|com.eg|com.et|com.fj|com.gh|com.gu|com.gn|com.gt|com.hk|com.jm|com.kh|com.kw|com.lb|com.lr|com.|com.|bd|mt|mv|ng|ni|np|nr|om|pa|py|qa|sa|sb|sg|sv|sy|tr|tw|ua|uy|ve|vi|vn|ye|coop|com.mt|com.mv|com.ng|com.ni|com.np|com.nr|com.om|com.pa|com.pl|com.py|com.qa|com.sa|com.sb|com.sv|com.sg|com.sy|com.tr|com.tw|com.ua|com.uy|com.ve|com.vi|com.vn|com.ye|cr|cu|cx|cv|cz|de|de.com|dj|dk|dm|do|dz|ec|edu|ee|es|eu|eu.com|fi|fo|fr|qa|qd|qf|gi|gl|gm|gp|gr|gs|gy|hk|hm|hr|ht|hu|ie|im|in|in.ua|io|ir|is|it|je|jo|jobs|jp|kg|ki|kn|kr|la|li|lk|lt|lu|lv|ly|ma|mc|md|me.uk|mg|mk|mo|mp|ms|mu|museum|mw|mx|my|na|nc|ne|nl|no|nf|nu|pe|ph|pk|pl|pn|pr|ps|pt|re|ro|rs|rw|sd|se|sg|sh|si|sk|sl|sm|sn|so|sr|st|sz|tc|td|tg|tj|tk|tl|tn|to|tt|tw|ug|us|vg|vn|vu|ws)";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            string oldName = player.Name;
            string newName = rgx.Replace(oldName, "").Trim();
            if (oldName != newName)
            {
                player.Rename(newName);
            }
        }
    }
}