import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    static outlets = ["profile-sidebar"]

    static values = {
        username: String
    }

    connect() {
        console.log("chat message controller loaded.");
    }

    showProfile(event) {
        event.preventDefault();
        console.log("It clicked!");
        if (this.hasProfileSidebarOutlet) {
            this.profileSidebarOutlet.show(this.usernameValue);
        } else {
            console.log("No outlet found");
        }
    }
}
