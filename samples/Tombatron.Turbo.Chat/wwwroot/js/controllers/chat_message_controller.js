import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    static outlets = ["profile-sidebar"]

    static values = {
        userId: String
    }

    showProfile(event) {
        event.preventDefault();
        if (this.hasProfileSidebarOutlet) {
            this.profileSidebarOutlet.show(this.userIdValue);
        }
    }
}
