---
title: Testimonials
---    
<br/>

<div class="container">
        <div class="row">
            <div class="col-md-2">
                <img class="img-circle" draggable="false" src="{{ 'ray-luzier-korn-drum-heads.jpg' | asset_url }}" alt="Ray Luzier" width="150" height="150">
            </div>
            <div class="col-md-6">
                <p>"This is one of the ultimate souvenirs that you could own by your favorite band. It looks so bad ass hangin' in my studio, <strong>like a kick drum's literally busting out of the wall!!</strong> My friends all trip out when they come over &amp; rant over it. Sticks, picks &amp; photos are cool..but get yourself a Select A Head, truly a unique display &amp; support of your fav band!!"</p>
                <p><strong>- Ray Luzier, KoRn, KXM</strong></p>
            </div>
        </div>
        <br/>
        <div class="row">
            <div class="col-md-2">
                <img class="img-circle" draggable="false" src="{{ 'keel.jpg' | asset_url }}" alt="Ron Keel" width="150" height="150">
            </div>
            <div class="col-md-6">
                <p>"We've all seen a cool new product for the first time and said 'I wish I'd thought of that.' Well, the folks at Select A Head have done just that - these beautiful displays are <strong>perfect for promoting and advertising just about anything, and also for decorating any home, office, recording studio, music venue, man cave or woman cave...</strong> I'm going to need a bigger house."</p>
                <p><strong>- Ron Keel, Ron Keel Band</strong></p>
            </div>
        </div>
      <br/>
        <div class="row">
            <div class="col-md-2">
                <img class="img-circle" draggable="false" src="{{ 'queensryche.jpg' | asset_url }}" alt="Queensryche" width="150" height="150">
            </div>
            <div class="col-md-6">
                <p>"My "Select A head" looks amazing, and has been a <strong>great conversation piece</strong> with all the film and music clients that come to my studio!!!"</p>
                <p><strong>- Scott Rockenfield, Queensryche Band</strong></p>
            </div>
        </div>
          <br/>
        <div class="row">
            <div class="col-md-2">
                <img class="img-circle" draggable="false" src="{{ 'eddie_trunk.jpg' | asset_url }}" alt="Eddie Trunk" width="150" height="150">
            </div>
            <div class="col-md-6">
                <p>"The Select a Head I have which was done with some of my logos is a truly cool collectible. I'm always asked by fellow rock fans where they can get such a <strong>great item for their favorite artists</strong>. Really very unique and striking way to fly the flag and represent the bands you love."</p>
                <p><strong>- Eddie Trunk, That Metal Show</strong></p>
            </div>
        </div>
       <br/>
        <div class="row">
            <div class="col-md-2">
                <img class="img-circle" draggable="false" src="{{ 'danielle_keister.jpg' | asset_url }}" alt="Danielle Keister" width="150" height="150">
            </div>
            <div class="col-md-6">
                <p>"Steve Kenkman and Select-a-Head are awesome! I had the pleasure of working with Steve in designing a head for one of my clients, a rock-n-roll attorney. They came up with a fantastic flame design circling my client's logo. It turned out so amazing! And working with Steve was so easy and so fun. And this is like a <strong>real drum head, not some cheesy, fake looking thing!</strong> My client now has it proudly displayed on his office wall. Love it!"</p>
                <p><strong>- Danielle Keister, The Relief Administrative</strong></p>
            </div>
        </div>
     <br/>
        <div class="row">
            <div class="col-md-2">
                <img class="img-circle" draggable="false" src="{{ 'pamela_moore.jpg' | asset_url }}" alt="Pamela Moore" width="150" height="150">
            </div>
            <div class="col-md-6">
                <p>"I LOVE my Select a Head custom design drum head! It's a unique way to show off my logo design. Their quality of work is of high standard, the colors are vivid and the mounting they use for the head, impressive! My experience with customer service has been exceptional, too! Select a Head LLC responds very quickly to my orders and goes above and beyond to help!<strong>Select a Head LLC ROCKS !!</strong>"</p>
                <p><strong>- Pamela Moore</strong></p>
            </div>
        </div>
</div>#array: [1,2,3,4,5,6]
{% for item in array limit:2 offset:2 %}
    {{ item }}
{% endfor %}