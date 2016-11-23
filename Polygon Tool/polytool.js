$(start);

function start()
{
	let canvas = $("#drawarea");
	canvas.css('background', 'lightgray');
	canvas.css('width', '1000');
	canvas.css('height', '500');
	canvas.click(addPoint);
	canvas.attr('width', 1000);
	canvas.attr('height', 500);

	$("#backbutton").click(undo);
	$("#resetbutton").click(resetlist);

	let list = [];
	let listOffset = [];

	$("#vertname").val("vert").keyup(update);
	resetlist();

	
	function addPoint(e)
	{
		let ctx = $('#drawarea')[0].getContext("2d");
		let pos = getMousePos($('#drawarea')[0], e);
		list.push(pos);

		update();
	}

	function clear()
	{
		let ctx = $('#drawarea')[0].getContext("2d");
		ctx.clearRect(0, 0, $('#drawarea')[0].width, $('#drawarea')[0].height);

		$("#pointlist li").remove();

		$("#exportbox").val("");

		listOffset = [];
	}
	function update()
	{
		clear();
		let ctx = $('#drawarea')[0].getContext("2d");

		let bgimg = document.getElementById("image");
        let canvas = document.getElementById("drawarea");
		ctx.drawImage(bgimg, canvas.width / 2 - bgimg.width / 2, canvas.height / 2 - bgimg.height / 2)
		
		for (var i = 1; i < list.length; i++) 
		{
			ctx.fillRect(list[i].x, list[i].y, 3, 3);

			if(list[i - 2] != undefined)
			{
				ctx.beginPath();
      			ctx.moveTo(list[i - 1].x + 1, list[i - 1].y + 1);
      			ctx.lineTo(list[i].x +1, list[i].y + 1);
      			ctx.stroke();
			}

			let offsetPos = {x: (list[i].x - list[0].x) * -1, y: (list[i].y - list[0].y) * -1}
			listOffset.push(offsetPos);

			$("#pointlist").append($("<li>" + "X: " + list[i].x + " / Y: " + list[i].y + "</li>"));
		}

		//Wrap around line.
		if(list.length > 2)
		{	
			ctx.beginPath();
      		ctx.moveTo(list[1].x, list[1].y);
      		ctx.lineTo(list[list.length - 1].x, list[list.length - 1].y);
      		ctx.stroke();		
		}

		let exporttext = [];

		for (var i = 0; i < listOffset.length; i++) {
			exporttext.push($("#vertname").val() + ".Add(new Vector2(" + listOffset[i].x + "," + listOffset[i].y + "));");
		}

		$("#exportbox").val(exporttext.join('\n'));



	}

	function undo()
	{
		if(list.length == 1) return;
		list = list.slice(0, list.length - 1);
		update();
	}
	function resetlist()
	{
		list = [];
		//Place center point.
		let canvasObj = document.getElementById("drawarea");
		list.push({x: canvasObj.width / 2,y: canvasObj.height / 2});
		update();
	}

	function getMousePos(canvas, evt) {
		var rect = canvas.getBoundingClientRect();
    	return {
        	x: Math.round((evt.clientX - rect.left) / (rect.right - rect.left) * canvas.width),
        	y: Math.round((evt.clientY - rect.top) / (rect.bottom - rect.top) * canvas.height)
    	};
	}
}