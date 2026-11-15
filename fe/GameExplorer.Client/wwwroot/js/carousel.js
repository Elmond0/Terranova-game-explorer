// endless carousel of GameSection: [copies of the last games] [real games] [copies of the first games].
// Reaching the copies jumps one lap back onto the real games: the content is the same, so the jump is invisible
window.carousel = {
		init(el, clones) {
			// recomputed every time because cards change width on resize.
			// getBoundingClientRect, not offsetLeft: offsetLeft rounds to integers, and with cards
			// e.g. 180.5px wide the row started on the copies and got stuck there.
			// realStart and realWidth are measured, not multiplied: half a pixel
			// of error, multiplied by 20 games, becomes 10px
			function metrics(){
				const x = i => el.children[i].getBoundingClientRect().left;
				const step = x(1) - x(0);
				const realStart = x(clones) - x(0);
				const realWidth = x(el.children.length - clones) - x(clones);
				return {step, realStart, realWidth};
			}

			el.scrollLeft = metrics().realStart;

			// returns the offset applied so a drag can follow it.
			// Half a card of margin: the browser rounds scrollLeft to whole pixels and an
			// exact comparison can miss by a fraction; this way a resting row is never on the edge
			function wrap() {
				const { step, realStart, realWidth } = metrics();
				if (el.scrollLeft < realStart - step / 2){
					el.scrollLeft += realWidth;
					return realWidth;
				}
				if (el.scrollLeft > realStart + realWidth - step / 2){
					el.scrollLeft -= realWidth;
					return -realWidth;
				}
				return 0;
			}

			// jump only when the row stops (120ms without scroll): jumping mid-swipe would break the swipe
			let timer;
			function onScrollEnd(){
				wrap();
				// dragging stays only while really dragging (button pressed and row moved):
				// holding the button without moving must not leave the closed hand cursor
				if (!down || !moved)
					el.classList.remove('dragging');
			}
			el.addEventListener('scroll', () => {
				clearTimeout(timer);
				timer = setTimeout(onScrollEnd, 120);
			});

			// mouse only: touch scrolling is already done by the browser
			let down = false;
			let moved = false;
			let startX = 0;
			let startScroll = 0;

			el.addEventListener('pointerdown', e => {
				if (e.pointerType !== 'mouse' || e.button !== 0){
					return;
				}
				down = true;
				startX = e.clientX;
				startScroll = el.scrollLeft;
				moved = false;
			});

			el.addEventListener('pointermove', e => {
				if (!down){
					return;
				}
				// button released outside the row: pointerup never arrives there, so it is noticed here
				if ((e.buttons & 1) === 0){
					endDrag();
					return;
				}
				const dx = e.clientX - startX;
				if (!moved){
					if (Math.abs(dx) < 5) return;   // under 5px it is a click with a shaky hand, not a drag
					moved = true;
					el.classList.add('dragging');
					el.setPointerCapture(e.pointerId);   // only now: capturing earlier keeps clicks from reaching the cards
				}
				el.scrollLeft = startScroll - dx;
				startScroll += wrap();   // when the row jumps, the drag start point must jump with it
			});

			function endDrag(){
				if (!down){
					return;
				}
				down = false;
				if (moved){
					const { step } = metrics();
					const target = Math.round(el.scrollLeft / step) * step;
					if (Math.abs(target - el.scrollLeft) < 1)   // with decimals, "equal" means "less than 1px apart"
						el.classList.remove('dragging');   // already in place: no scroll, so onScrollEnd would never run
					else
						el.scrollTo({ left: target, behavior: 'smooth' });
				}
			}
			el.addEventListener('pointerup', endDrag);
			el.addEventListener('pointercancel', endDrag);

			// releasing after a drag must not open the game (true: runs before the link and Blazor)
			el.addEventListener('click', e => {
				if (moved){
					e.preventDefault();
					e.stopPropagation();
					moved = false;
				}
			}, true);

			// Firefox ignores -webkit-user-drag, so its image drag is blocked here
			el.addEventListener('dragstart', e => e.preventDefault());
		}
};
