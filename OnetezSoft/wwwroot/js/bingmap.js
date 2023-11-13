async function GetMap(key, obj, longlat) {

  if (document.getElementById("myMap") == null) return;
  if (!key) return;
  try {
    var dotNet = obj;
    var location = longlat;
    if (!longlat) {
      location = await window.getLocation();
    }
    // Initialized Map
    var map = new Microsoft.Maps.Map('#myMap', {
      credentials: key,
      zoom: 18,
      center: new Microsoft.Maps.Location(location[0], location[1]),
      trafficOptions: {
        flowVisible: false,
        opacity: 0,
      }
    });

    // Info box
    var infobox = new Microsoft.Maps.Infobox(map.getCenter(), {
      visible: false
    });
    infobox.setMap(map);

    // Pushpin
    var pushpin = new Microsoft.Maps.Pushpin(map.getCenter(), null);
    pushpin.setOptions({
      enableHoverStyle: true,
      draggable: true,
    });
    pushpin.metadata = {
      title: "Vị trí công ty",
    };
    //Add a click event handler to the pushpin.
    Microsoft.Maps.Events.addHandler(pushpin, 'click', pushpinClicked);
    Microsoft.Maps.Events.addHandler(pushpin, 'dragend', pinDragEnd);

    map.entities.push(pushpin);

    // Search
    var searchbar = document.getElementById("mapSearch");
    var searchInput;
    var searchButton;

    // Draw circle for distance
    var distanceValue = document.getElementById("mapDistance");
    var distance = 0;
    var polygon;

    if (distanceValue) {
      distanceValue.addEventListener("keyup", (e, v) => {
        if (e.key === 'Enter' || e.keyCode === 13) {
          CallbackDotNet();
        }
      });
      distanceValue.addEventListener("blur", () => {
        CallbackDotNet();
      })
    }

    Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
      updatePolygon();
    });

    if (searchbar) {
      searchInput = searchbar.querySelector("input");
      searchButton = searchbar.querySelector(".icon");

      if (searchInput) {
        searchInput.addEventListener("keyup", (e, v) => {
          if (e.key === 'Enter' || e.keyCode === 13) {
            geocodeQuery();
          }
        })
      }
      searchButton.addEventListener("click", geocodeQuery);
    }
    var searchManager;
    Microsoft.Maps.loadModule('Microsoft.Maps.Search', function () {
      searchManager = new Microsoft.Maps.Search.SearchManager(map);
    });

    // Pushpin method
    function pushpinClicked(e) {
      //Make sure the infobox has metadata to display.
      if (e.target.metadata) {
        //Set the infobox options with the metadata of the pushpin.
        infobox.setOptions({
          location: e.target.getLocation(),
          title: e.target.metadata.title,
          visible: true
        });
      }
    }

    function pinDragEnd(e) {
      var loc = e.location;
      location = [loc.latitude, loc.longitude];
      CallbackDotNet();
    }

    // Search Method
    function geocodeQuery() {
      var query = searchInput.value;
      //If search manager is not defined, load the search module.
      var searchRequest = {
        where: query,
        callback: function (r) {
          //Add the first result to the map and zoom into it.
          if (r && r.results && r.results.length > 0) {
            var loc = r.results[0].location;
            pushpin.setLocation(loc);

            map.setView({ bounds: r.results[0].bestView });

            searchInput.value = loc.name;
            location = [loc.latitude, loc.longitude];
            CallbackDotNet();
          }
        },
        errorCallback: function (e) {
          //If there is an error, alert the user about it.
          alert("No results found.");
        }
      };
      //Make the geocode request.
      searchManager.geocode(searchRequest);
    }

    // Create Polygon
    function updatePolygon() {
      var distanceUnit;
      if (Microsoft.Maps && Microsoft.Maps.SpatialMath && Microsoft.Maps.SpatialMath.DistanceUnits) {
        distanceUnit = Microsoft.Maps.SpatialMath.DistanceUnits.Meters;
      }
      else {
        return;
      }
      var sides = 36;
      distance = parseInt(distanceValue.value);
      var circle = Microsoft.Maps.SpatialMath.getRegularPolygon(pushpin.getLocation(), distance, sides, distanceUnit);

      if (!polygon) {
        polygon = new Microsoft.Maps.Polygon(circle);
        polygon.setOptions({
          fillColor: new Microsoft.Maps.Color(50, 0, 255, 0),
          strokeColor: new Microsoft.Maps.Color(80, 0, 255, 0),
          strokeThickness: 2
        })

        map.entities.push(polygon);
      }
      else {
        polygon.setLocations(circle);
      }
    }

    // Test distance
    //var end = await getLocation();
    //var endLoc = new Microsoft.Maps.Location(end[0], end[1]);
    //var pushpin2 = new Microsoft.Maps.Pushpin(endLoc, null);

    //pushpin2.metadata = {
    //   title: "Vị trí của tôi",
    //};

    //map.entities.push(pushpin2)

    //var result = await getDistance(location, distanceValue.value);

    //console.log(result);

    // Callback method
    function CallbackDotNet() {
      updatePolygon();
      dotNet.invokeMethodAsync("SetLocation", location, distance);
    }
  }
  catch (e) {
    console.log(e)
  }
}