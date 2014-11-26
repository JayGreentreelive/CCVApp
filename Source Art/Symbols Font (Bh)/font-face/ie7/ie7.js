/* To avoid CSS expressions while still supporting IE 7 and IE 6, use this script */
/* The script tag referring to this file must be placed before the ending body tag. */

/* Use conditional comments in order to target IE 7 and older:
	<!--[if lt IE 8]><!-->
	<script src="ie7/ie7.js"></script>
	<!--<![endif]-->
*/

(function() {
	function addIcon(el, entity) {
		var html = el.innerHTML;
		el.innerHTML = '<span style="font-family: \'Bh\'">' + entity + '</span>' + html;
	}
	var icons = {
		'icon-Bollhavet_headphones': '&#xe600;',
		'icon-Bollhavet_stop': '&#xe601;',
		'icon-Bollhavet_next': '&#xe602;',
		'icon-Bollhavet_pause': '&#xe603;',
		'icon-Bollhavet_play': '&#xe604;',
		'icon-Bollhavet_previous': '&#xe605;',
		'icon-Bollhavet_rewind': '&#xe606;',
		'icon-Bollhavet_attached-file': '&#xe607;',
		'icon-Bollhavet_fast-forward': '&#xe608;',
		'icon-Bollhavet_music': '&#xe609;',
		'icon-Bollhavet_video': '&#xe60a;',
		'icon-Bollhavet_bell': '&#xe60b;',
		'icon-Bollhavet_guitar-335': '&#xe60c;',
		'icon-Bollhavet_guitar-explorer': '&#xe60d;',
		'icon-Bollhavet_view': '&#xe60e;',
		'icon-Bollhavet_arrow-small-left': '&#xe60f;',
		'icon-Bollhavet_hedgehog': '&#xe610;',
		'icon-Bollhavet_picture': '&#xe611;',
		'icon-Bollhavet_question': '&#xe612;',
		'icon-Bollhavet_arrow-small-right': '&#xe613;',
		'icon-Bollhavet_lock-closed': '&#xe614;',
		'icon-Bollhavet_user-multiple': '&#xe615;',
		'icon-Bollhavet_user': '&#xe616;',
		'icon-Bollhavet_bookmark': '&#xe617;',
		'icon-Bollhavet_calendar': '&#xe618;',
		'icon-Bollhavet_lock-open': '&#xe619;',
		'icon-Bollhavet_trend-negative': '&#xe61a;',
		'icon-Bollhavet_trend-positive': '&#xe61b;',
		'icon-Bollhavet_clock': '&#xe61c;',
		'icon-Bollhavet_export': '&#xe61d;',
		'icon-Bollhavet_external-link': '&#xe61e;',
		'icon-Bollhavet_info': '&#xe61f;',
		'icon-Bollhavet_chevron-left': '&#xe620;',
		'icon-Bollhavet_chevron-right': '&#xe621;',
		'icon-Bollhavet_staples-diagram': '&#xe622;',
		'icon-Bollhavet_trashcan': '&#xe623;',
		'icon-Bollhavet_curve': '&#xe624;',
		'icon-Bollhavet_doc-add': '&#xe625;',
		'icon-Bollhavet_flag': '&#xe626;',
		'icon-Bollhavet_send': '&#xe627;',
		'icon-Bollhavet_doc-excel': '&#xe628;',
		'icon-Bollhavet_doc-remove': '&#xe629;',
		'icon-Bollhavet_doc-text': '&#xe62a;',
		'icon-Bollhavet_doc': '&#xe62b;',
		'icon-Bollhavet_lightbulb': '&#xe62c;',
		'icon-Bollhavet_home': '&#xe62d;',
		'icon-Bollhavet_refresh': '&#xe62e;',
		'icon-Bollhavet_remove': '&#xe62f;',
		'icon-Bollhavet_warning': '&#xe630;',
		'icon-Bollhavet_add': '&#xe631;',
		'icon-Bollhavet_check': '&#xe632;',
		'icon-Bollhavet_delete': '&#xe633;',
		'icon-Bollhavet_globe': '&#xe634;',
		'icon-Bollhavet_cart': '&#xe635;',
		'icon-Bollhavet_edit': '&#xe636;',
		'icon-Bollhavet_like': '&#xe637;',
		'icon-Bollhavet_menu-expand': '&#xe638;',
		'icon-Bollhavet_menu': '&#xe639;',
		'icon-Bollhavet_options': '&#xe63a;',
		'icon-Bollhavet_pin': '&#xe63b;',
		'icon-Bollhavet_search': '&#xe63c;',
		'icon-Bollhavet_share': '&#xe63d;',
		'icon-Bollhavet_heart': '&#xe63e;',
		'icon-Bollhavet_mail': '&#xe63f;',
		'icon-Bollhavet_phone': '&#xe640;',
		'icon-Bollhavet_star': '&#xe641;',
		'icon-Bollhavet_user-female': '&#xe642;',
		'icon-Bollhavet_arrow-thin-left': '&#xe643;',
		'icon-Bollhavet_arrow-thin-right': '&#xe644;',
		'icon-Bollhavet_upload': '&#xe645;',
		'icon-Bollhavet_user-male': '&#xe646;',
		'icon-Bollhavet_bird': '&#xe647;',
		'icon-Bollhavet_download': '&#xe648;',
		'icon-Bollhavet_message': '&#xe649;',
		'0': 0
		},
		els = document.getElementsByTagName('*'),
		i, c, el;
	for (i = 0; ; i += 1) {
		el = els[i];
		if(!el) {
			break;
		}
		c = el.className;
		c = c.match(/icon-[^\s'"]+/);
		if (c && icons[c[0]]) {
			addIcon(el, icons[c[0]]);
		}
	}
}());
