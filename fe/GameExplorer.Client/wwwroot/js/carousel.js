// Carosello di GameSection: [copie ultimi giochi] [giochi veri] [copie primi giochi].
// Arrivati sulle copie si salta di un giro sui giochi veri: il salto non si vede.
window.carousel = {
		init(el, clones) {
			// 1. misure (ricalcolate: le card cambiano larghezza col ridimensionamento).
			//    getBoundingClientRect e non offsetLeft: offsetLeft arrotonda all'intero, e con card
			//    larghe ad es. 180,5 px la riga partiva sulle copie e lì restava bloccata.
			//    realStart e realWidth si misurano, non si moltiplicano: anche mezzo pixel
			//    di errore, moltiplicato per 20 giochi, diventa 10 px
			function metrics(){
				const x = i => el.children[i].getBoundingClientRect().left;     // bordo sinistro della card i
				const step = x(1) - x(0);                                        // una card + lo spazio
				const realStart = x(clones) - x(0);                              // dove inizia il primo gioco vero
				const realWidth = x(el.children.length - clones) - x(clones);    // un giro: tutti i giochi veri
				return {step, realStart, realWidth};
			}

			// 2. si parte dal primo gioco vero
			el.scrollLeft = metrics().realStart;

			// 3. se la riga è sulle copie salta di un giro; restituisce lo spostamento fatto.
			//    Margine di mezza card: il browser arrotonda scrollLeft ai pixel interi e un
			//    confronto esatto può sbagliare di una frazione; così una riga ferma non è mai in bilico
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

			// 4. fine scorrimento: si salta solo a riga ferma (120 ms senza scroll)
			let timer;
			function onScrollEnd(){
				wrap();
				// dragging resta solo mentre si sta trascinando (tasto premuto e riga mossa):
				// chi tiene premuto senza muovere non deve restare con la manina chiusa
				if (!down || !moved)
					el.classList.remove('dragging');
			}
			el.addEventListener('scroll', () => {
				clearTimeout(timer);
				timer = setTimeout(onScrollEnd, 120);
			});

			// 5. trascinamento col mouse (il touch lo fa già il browser)
			let down = false;      // tasto premuto
			let moved = false;     // trascinamento, non click
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
				// tasto rilasciato fuori dalla riga: lì il pointerup non arriva, se ne accorge ora
				if ((e.buttons & 1) === 0){
					endDrag();
					return;
				}
				const dx = e.clientX - startX;
				if (!moved){
					if (Math.abs(dx) < 5) return;   // sotto i 5 px è un click
					moved = true;
					el.classList.add('dragging');
					el.setPointerCapture(e.pointerId);   // solo ora, o i click non arrivano alle card
				}
				el.scrollLeft = startScroll - dx;
				startScroll += wrap();   // se la riga salta, salta anche il punto di partenza
			});

			// al rilascio si aggancia alla card più vicina; dragging la toglie onScrollEnd
			function endDrag(){
				if (!down){
					return;
				}
				down = false;
				if (moved){
					const { step } = metrics();
					const target = Math.round(el.scrollLeft / step) * step;
					if (Math.abs(target - el.scrollLeft) < 1)   // con i decimali, "uguale" è "a meno di 1 px"
						el.classList.remove('dragging');   // già al suo posto: nessuno scroll
					else
						el.scrollTo({ left: target, behavior: 'smooth' });
				}
			}
			el.addEventListener('pointerup', endDrag);
			el.addEventListener('pointercancel', endDrag);

			// 6. il rilascio dopo un trascinamento non apre il gioco (true: prima del link e di Blazor)
			el.addEventListener('click', e => {
				if (moved){
					e.preventDefault();
					e.stopPropagation();
					moved = false;
				}
			}, true);

			// niente "trascina immagine" (Firefox)
			el.addEventListener('dragstart', e => e.preventDefault());
		}
};
