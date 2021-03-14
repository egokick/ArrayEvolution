var queue = document.getElementById('queue').children;

var dict = {};
for(var i=0; i< queue.length; i++)
{
    var name = queue[i].title;
    var videoTime = queue[i].getElementsByClassName('qe_time')[0].innerHTML;
    console.log(videoTime);
    if(isEmpty(dict[name]))
    {
        dict[name] = videoTime;
    }
    else 
    {
        var totalTime = addYoutubeTime(dict[name], videoTime);
        dict[name] = totalTime;
    }
}
 
// write out to page
document.getElementById("leftpane").innerHTML = JSON.stringify(dict)


function addYoutubeTime(time1, time2)
{
	var time1Hour = 0;
	var time1Minutes = 0;
	var time1Seconds = 0;
	var time2Hour = 0;
	var time2Minutes = 0;
	var time2Seconds = 0;

	var totalSeconds;
	var totalMinutes;
	var totalHours; 
  
	if(time1.length == 8) 
  
	{
		time1Hour = parseInt(time1.substring(0, 2));
		time1Minutes = parseInt(time1.substring(3, 5));	
		time1Seconds = parseInt(time1.substring(6, 8));
	} 
	if(time1.length == 5) //minutes:seconds
	{
		time1Minutes = parseInt(time1.substring(0, 2));	    
		time1Seconds = parseInt(time1.substring(3, 5)); 		
	}
	if(time1.length == 2) //seconds
	{
		time1Seconds = parseInt(time1.substring(0, 2)); 
	}

	if(time2.length == 8) 
	{
		time2Hour = parseInt(time2.substring(0, 2));    
		time2Minutes = parseInt(time2.substring(3, 5));	    
		time2Seconds = parseInt(time2.substring(6, 8));    
	} 
	if(time2.length == 5) //minutes:seconds
	{
		time2Minutes = parseInt(time2.substring(0, 2));	    
		time2Seconds = parseInt(time2.substring(3, 5)); 		
	}
	if(time2.length == 2) //seconds
	{
		time2Seconds = time2.substring(0, 2); 
	}
	var d = new Date();  
	d.setHours(time1Hour + time2Hour, time1Minutes + time2Minutes, time1Seconds + time2Seconds);	
  
	return d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
}

function isEmpty(obj) {
    for(var key in obj) {
        if(obj.hasOwnProperty(key))
            return false;
    }
    return true;
}
